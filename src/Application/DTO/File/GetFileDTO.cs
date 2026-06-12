using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.File {
    public class GetFileDTO {
        public required Guid Id { get; init; }
        [Description("Имя без расширения")]
        [MaxLength(255)]
        public string? Name { get; set; }
        [Description("Расширение")]
        public required string Extension { get; set; }
        [Description("Размер")]
        public required long Size { get; set; }
        public required DateTime CreatedAt { get; set; }
        [Description(@"⚡ Путь к файлу  
            Полный URL: http://{{api_base}}/Uploads/{{path}}")]
        public required string Path { get; set; }

        public static GetFileDTO FromEntity(Domain.Models.File entity) => new() {
            Id = entity.Id,
            Name = entity.Name,
            Extension = entity.Extension,
            Size = entity.Size,
            CreatedAt = entity.CreatedAt,
            Path = entity.Path
        };
    }
}
