using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services.Interfaces {
    public interface IFileService {
        IQueryable<Domain.Models.File> All();
        Task<Domain.Models.File> CreateFileRecord(Domain.Models.File entity);
        Task DeleteFile(Domain.Models.File entity);
        Task<Domain.Models.File?> Find(Guid id);
        Task<Results<Ok<Domain.Models.File>, ValidationProblem>> UploadFile(IFormFile form, FileType type, Guid user_id);
    }
}