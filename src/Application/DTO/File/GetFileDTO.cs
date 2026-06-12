using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.File {
    public class GetFileDTO {
        public required Guid Id { get; init; }
        [Description("Name without extension")]
        [MaxLength(255)]
        public string? Name { get; set; }
        [Description("Extension")]
        public required string Extension { get; set; }
        [Description("Size")]
        public required long Size { get; set; }
        public required DateTime CreatedAt { get; set; }
        [Description(@"⚡ File path
            Full URL: http://{{api_base}}/Uploads/{{path}}")]
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
