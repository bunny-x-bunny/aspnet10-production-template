using Application.DTO.Notification;
using Application.Services.Interfaces;
using Application.SignalR;
using Application.SignalR.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Services {
  public class NotificationService(AppDbContext context, IHubContext<MainHub, IMainHub> ws) : INotificationService {
    private readonly AppDbContext context = context;
    private readonly IHubContext<MainHub, IMainHub> ws = ws;

    public IQueryable<Notification> All() => context.Notifications;

    public async Task<Notification> Create(CreateNotificationDTO request, bool with_save = false) {
      var entity = request.ToEntity();
      context.Notifications.Add(entity);
      if (with_save)
        await context.SaveChangesAsync();
      await ws.Clients.User(request.User.ToString()).New(GetNotificationDTO.FromEntity(entity));
      return entity;
    }

    public async Task Delete(Notification entity) {
      context.Notifications.Remove(entity);
      await context.SaveChangesAsync();
    }

    public async Task<Notification?> Find(Guid uuid) => await All().FirstOrDefaultAsync(n => n.Id == uuid);

    public async Task UpdateRead(IQueryable<Notification> entities, bool read)
        => await entities.ExecuteUpdateAsync(n => n.SetProperty(n => n.Read, read));
  }
  public static class NotificationServiceExt {
    public static IQueryable<Notification> ForUser(this IQueryable<Notification> query, Guid user_id)
        => query.Where(n => n.UserId == user_id);
  }
}
