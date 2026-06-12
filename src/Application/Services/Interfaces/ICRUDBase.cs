using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services.Contracts {
    public interface ICRUDBase<T, TCreateRequest, TUpdateRequest> {
        IQueryable<T> All();
        Task<T?> Find(Guid uuid);
        Task<T> Create(TCreateRequest request);
        Task<T> Update(T entity, TUpdateRequest request);
        Task Delete(T entity);
    }
}
