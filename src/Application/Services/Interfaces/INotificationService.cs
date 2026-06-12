using Application.DTO.Notification;
using Domain.Models;

namespace Application.Services.Interfaces {
    public interface INotificationService {
        IQueryable<Notification> All();
        Task<Notification> Create(CreateNotificationDTO request, bool with_save = false);
        Task Delete(Notification entity);
        Task<Notification?> Find(Guid uuid);
        Task UpdateRead(IQueryable<Notification> entities, bool read);
    }
}