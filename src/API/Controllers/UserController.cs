using API.Extensions;
using API.Helpers;
using Application.DTO;
using Application.DTO.User;
using Application.DTO.User.Search;
using Application.Services.Interfaces;
using Common.Extensions;
using Domain.Enum;
using Domain.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MR.AspNetCore.Pagination;
using System.ComponentModel;

namespace API.Controllers {
  [Route("[controller]")]
  [ApiController]
  public class UserController(IUserService service, IPaginationService pagination) : ControllerBase {
    private readonly IUserService service = service;
    private readonly IPaginationService pagination = pagination;

    [HttpGet]
    [EndpointDescription("All users")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<KeysetPaginationResult<GetUserExtDTO>> GetAll(
        [FromQuery] KeysetQueryModel _,
        [FromQuery] UserSearchModel search,
        [FromQuery] SortModel sort
    ) => await pagination.KeysetPaginateAsync(
      service.All().Where(search.ToPredicate<AppUser>()),
      sort.ToExpression<AppUser>().Compile(),
      async uuid => await service.Find(new Guid(uuid)),
      service.Project
    );

    [HttpGet("self")]
    [EndpointDescription("Current user info")]
    [Authorize]
    public async Task<Results<Ok<GetUserExtDTO>, NotFound>> GetSelf()
      => await service.Find((Guid)User.Uuid()!) switch {
        AppUser x => TypedResults.Ok(GetUserExtDTO.FromEntity(x)),
        _ => TypedResults.NotFound()
      };

    [HttpPut("self/basic")]
    [EndpointDescription("Update current user's basic info")]
    [Authorize]
    public async Task<Results<Ok<GetUserBasicDTO>, UnauthorizedHttpResult>> UpdateSelf([FromBody] UserBasicDTO request)
      => (await service.UpdateBasic((Guid)User.Uuid()!, request)).Result switch {
        Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
        NotFound __ => TypedResults.Unauthorized()
      };

    /*[HttpPut("self/lang")]
    [EndpointDescription("Change notification language")]
    [Authorize]
    public async Task UpdateLang([FromBody][Description("Language (ISO-2)")][MaxLength(2)][RegularExpression(@"^[a-z]{2}$")] string lang)
        => await service.UpdateLang((Guid)User.Uuid()!, lang);*/

    [HttpGet("{uuid}")]
    [EndpointDescription("User info")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserExtDTO>, NotFound>> Get(Guid uuid)
      => await service.Find(uuid) switch {
        AppUser x => TypedResults.Ok(GetUserExtDTO.FromEntity(x)),
        _ => TypedResults.NotFound()
      };

    [HttpPatch("{uuid}/credentials")]
    [EndpointDescription(@"Update user credentials
            At least one parameter is sufficient")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserBasicDTO>, ValidationProblem, NotFound>> UpdateCredentials(Guid uuid, [FromBody] UpdateUserCredentialsDTO request)
        => (await service.UpdateCredentials(uuid, request)).Result switch {
          Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
          ValidationProblem __ => __,
          NotFound __ => __
        };

    [HttpPost("Admin")]
    [EndpointDescription("Create admin")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserBasicDTO>, ValidationProblem>> CreateAdmin([FromBody] CreateAdminDTO request)
        => (await service.CreateUser(request.ToEntity(), request.Email, request.Password)).Result switch {
          Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
          ValidationProblem __ => __
        };

    [HttpPost("User")]
    [EndpointDescription("Create user")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserBasicDTO>, ValidationProblem>> CreateUser([FromBody] CreateUserDTO request)
        => (await service.CreateUser(request.ToEntity(), request.Email, request.Password)).Result switch {
          Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
          ValidationProblem __ => __
        };

    [HttpPut("Admin/{uuid}")]
    [EndpointDescription("Update admin")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserBasicDTO>, NotFound>> UpdateAdmin(Guid uuid, [FromBody] UpdateUserDTO request)
        => (await service.UpdateAdmin(uuid, request)).Result switch {
          Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
          NotFound __ => __
        };

    [HttpPut("User/{uuid}")]
    [EndpointDescription("Update user")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok<GetUserBasicDTO>, NotFound>> UpdateUser(Guid uuid, [FromBody] UpdateUserDTO request)
        => (await service.UpdateUser(uuid, request)).Result switch {
          Ok<AppUser> { Value: var x } => TypedResults.Ok(GetUserBasicDTO.FromEntity(x!)),
          NotFound __ => __
        };

    [HttpDelete("{uuid}")]
    [EndpointDescription(@"Delete a user and all related data")]
    [AuthorizeJWTRoles(Role.Admin)]
    public async Task<Results<Ok, NotFound>> Delete(Guid uuid, [FromQuery] [Description("Delete uploaded files")] bool delete_files) {
      if (await service.Find(uuid) is not AppUser entity) return TypedResults.NotFound();
      await service.Delete(entity, delete_files);
      return TypedResults.Ok();
    }
  }
}
