using API.Extensions;
using API.Helpers;
using Application.DTO;
using Application.DTO.File;
using Application.Helpers;
using Application.Services.Interfaces;
using Common.Extensions;
using Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MR.AspNetCore.Pagination;
using System.ComponentModel;

namespace API.Controllers {
  [Route("[controller]")]
  [ApiController]
  public class FileController(IFileService service, IUserService user_service, IPaginationService pagination) : ControllerBase {
    private readonly IFileService service = service;
    private readonly IUserService user_service = user_service;
    private readonly IPaginationService pagination = pagination;

    [HttpGet]
    [EndpointDescription("Все файлы")]
    [Authorize]
    public async Task<KeysetPaginationResult<GetFileDTO>> GetAll(
      [FromQuery] KeysetQueryModel _,
      [FromQuery] FileSearchModel search,
      [FromQuery] SortModel sort
    ) {
      if (User.Roles().First() is not Role.Admin)
        search.User = (Guid)User.Uuid()!;

      return await pagination.KeysetPaginateAsync(
          service.All().Where(search.ToPredicate()),
          sort.ToExpression<Domain.Models.File>().Compile(),
          async uuid => await service.Find(new Guid(uuid)),
          q => q.Select(e => GetFileDTO.FromEntity(e))
      );
    }

    [HttpPost("{type}")]
    [EndpointDescription(@"Загрузка файла  
      Ограничения:  
      `jpg, jpeg, png, webp` -> 2МБ")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetFileDTO>, ValidationProblem>> Upload(IFormFile file, [Description("Назначение файла")] FileType type) {
      if (type is FileType.UserAvatar)
        throw new InvalidOperationException("Use POST /File/self/avatar instead");

      var ext = Path.GetExtension(file.FileName).Trim('.').ToLower();
      switch (ext, type) {
        case ("jpg" or "jpeg" or "png" or "webp", _): {
          if (file.Length >= 2 * 1024 * 1024)
            return Validation.CreateValidationProblem("FileTooLarge", "Размер файла должен быть менее 2МБ");
          break;
        }
        default:
          return Validation.CreateValidationProblem("InvalidFileExtension", "Некорректное расширение файла");
      }
      switch ((await service.UploadFile(file, type, (Guid)User.Uuid()!)).Result) {
        case Ok<Domain.Models.File> { Value: var x }:
          return TypedResults.Ok(GetFileDTO.FromEntity(await service.CreateFileRecord(x!)));
        case ValidationProblem __: return __;
        default: throw new Exception();
      };
    }

    [HttpPut("self/avatar")]
    [EndpointDescription(@"Изменение своего аватара  
      Ограничения:  
      `jpg, jpeg, png, webp` -> 2МБ")]
    [Authorize]
    public async Task<Results<Ok<GetFileDTO?>, ValidationProblem, UnauthorizedHttpResult>> UpdateAvatar(IFormFile? file) {
      // perform validations
      if (await user_service.Find((Guid)User.Uuid()!) is not { } user)
        return TypedResults.Unauthorized();
      if (file is not null && Path.GetExtension(file.FileName).Trim('.').ToLower() is string @ext) {
        if (file.Length >= 2 * 1024 * 1024)
          return Validation.CreateValidationProblem("FileTooLarge", "Размер изображения должен быть менее 2МБ");
        if (!new[] { "jpg", "jpeg", "png", "webp" }.Contains(ext))
          return Validation.CreateValidationProblem("InvalidExtention", "Некорректное разрешение файла");
      }

      // delete existing avatar
      if (user.Avatar is not null)
        await service.DeleteFile(user.Avatar);

      // set new avatar
      if (file is null) {
        await user_service.UpdateAvatar(user, null);
        return TypedResults.Ok<GetFileDTO?>(null);
      } else switch ((await service.UploadFile(file, FileType.UserAvatar, (Guid)User.Uuid()!)).Result) {
          case Ok<Domain.Models.File> { Value: var file_record }:
            await service.CreateFileRecord(file_record!);
            await user_service.UpdateAvatar(user, file_record!);
            return TypedResults.Ok(GetFileDTO.FromEntity(file_record!))!;
          case ValidationProblem __: return __;
        };
      throw new Exception("UnexhaustiveSwitchExpression");
    }

    [HttpDelete("{uuid}")]
    [EndpointDescription("Удаление файла и всех связей")]
    [Authorize]
    public async Task<Results<Ok, ForbidHttpResult, NotFound>> DeleteFile(Guid uuid) {
      if (await service.Find(uuid) is not { } entity)
        return TypedResults.NotFound();
      if (User.Roles().First() is not Role.Admin && entity.UserId != User.Uuid())
        return TypedResults.Forbid();
      await service.DeleteFile(entity);
      return TypedResults.Ok();
    }
  }
}
