using Application.Helpers;
using Application.Services.Interfaces;
using Domain.Enum;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Persistence;

namespace Application.Services {
  public class FileService(AppDbContext context, IOptions<AppSettings> settings) : IFileService {
    private readonly AppDbContext context = context;
    private readonly IOptions<AppSettings> settings = settings;

    public IQueryable<Domain.Models.File> All() => context.Files;

    public async Task<Domain.Models.File?> Find(Guid id) => await context.Files.FindAsync(id);

    public async Task<Domain.Models.File> CreateFileRecord(Domain.Models.File entity) {
      context.Files.Add(entity);
      await context.SaveChangesAsync();
      return entity;
    }

    public async Task<Results<Ok<Domain.Models.File>, ValidationProblem>> UploadFile(IFormFile form, FileType type, Guid user_id) {
      if (form.Length == 0)
        return Validation.CreateValidationProblem("MissingData", "Размер файла должен быть более 0Б");
      var file_record = new Domain.Models.File {
        Id = Guid.CreateVersion7(),
        Type = type,
        UserId = user_id,
        Size = form.Length,
        Name = Path.GetFileNameWithoutExtension(form.FileName),
        Extension = Path.GetExtension(form.FileName).Trim('.').ToLower(),
      };
      if (form.Length >= 128 * 1024 * 1024)
        return Validation.CreateValidationProblem("FileTooLarge", "Размер файла должен быть менее 128МБ");
      var dir = file_record.Extension switch {
        "mp4" => $"{file_record.Type}_Temp",
        _ => file_record.Dir
      };
#if LINUX
      // Set permissions to rwxr-xr-- (754 in octal)
      const UnixFileMode chmod =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
        UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
        UnixFileMode.OtherRead | UnixFileMode.OtherExecute;
      Directory.CreateDirectory(Path.Combine(settings.Value.UploadPath, dir), chmod);
#else
      Directory.CreateDirectory(Path.Combine(settings.Value.UploadPath, dir));
#endif
      var path = Path.Combine(settings.Value.UploadPath, dir, file_record.PhysicalFileName);
      using var stream = System.IO.File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
      await form.CopyToAsync(stream);
      return TypedResults.Ok(file_record);
    }

    public async Task DeleteFile(Domain.Models.File entity) {
      var path = Path.Combine(settings.Value.UploadPath, entity.Path);
      System.IO.File.Delete(path);
      context.Files.Remove(entity);
      await context.SaveChangesAsync();
    }
  }
}
