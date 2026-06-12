using API.Extensions;
using Application.DTO;
using Application.DTO.Notification;
using Application.DTO.Notification.Search;
using Application.DTO.Notification.Types;
using Application.Services;
using Application.Services.Interfaces;
using Common.Extensions;
using Domain.Enum;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MR.AspNetCore.Pagination;
using System.ComponentModel;

namespace API.Controllers {
  [Route("[controller]")]
  [ApiController]
  public class NotificationController(INotificationService service, IPaginationService pagination) : ControllerBase {
    private readonly INotificationService service = service;
    private readonly IPaginationService pagination = pagination;

    [HttpGet("my")]
    [EndpointDescription("Мои уведомления")]
    [Authorize]
    public async Task<KeysetPaginationResult<GetNotificationDTO>> GetAllMy(
        [FromQuery] KeysetQueryModel _,
        [FromQuery] NotificationSearchModel search,
        [FromQuery] SortModel sort
    ) => await pagination.KeysetPaginateAsync(
        service.All()
            .ForUser((Guid)User.Uuid()!)
            .Where(search.ToPredicate()),
        sort.ToExpression<Notification>().Compile(),
        async uuid => await service.Find(new Guid(uuid)),
        q => q.Select(e => GetNotificationDTO.FromEntity(e))
    );

    [HttpGet("{uuid}")]
    [EndpointDescription("Конкретное уведомление")]
    [Authorize]
    public async Task<Results<Ok<GetNotificationDTO>, NotFound>> Get(Guid uuid)
        => await service.All()
        .ForUser((Guid)User.Uuid()!)
        .SingleOrDefaultAsync(n => n.Id == uuid) switch {
          Notification x => TypedResults.Ok(GetNotificationDTO.FromEntity(x)),
          _ => TypedResults.NotFound()
        };

    [HttpPost("read/{read}")]
    [EndpointDescription("Флаг прочтения уведомлений")]
    [Authorize]
    public async Task ConfirmRead(bool read, [FromBody][Description("ID уведомлений")] IEnumerable<Guid> request)
        => await service.UpdateRead(
            service.All()
                .ForUser((Guid)User.Uuid()!)
                .Where(n => request.Contains(n.Id)),
            read
        );

    [HttpDelete("{uuid}")]
    [EndpointDescription("Удаление уведомления")]
    [Authorize]
    public async Task<Results<Ok, NotFound>> Delete(Guid uuid) {
      if (await service.All()
          .ForUser((Guid)User.Uuid()!)
          .SingleOrDefaultAsync(n => n.Id == uuid) is not { } entity)
        return TypedResults.NotFound();
      await service.Delete(entity);
      return TypedResults.Ok();
    }

    [HttpGet($".doc/{nameof(NotificationType.UserRegistered)}")] public Task<NotificationDocProjection<UserRegisteredNotification>> UserRegistered() => throw new NotImplementedException();
  }
}
