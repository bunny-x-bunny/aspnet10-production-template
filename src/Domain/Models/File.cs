using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models {
    public class File {
        [Key]
        public Guid Id { get; init; }
        [Description("Тип")]
        public required FileType Type { get; set; }
        [Description("Создатель файла")]
        public Guid? UserId { get; set; }
        public AppUser? User { get; set; } = null!;
        [Description("Имя без расширения")]
        [MaxLength(255)]
        public string? Name { get; set; }
        [Description("Расширение")]
        public required string Extension { get; set; }
        [Description("Размер")]
        public required long Size { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Description(@"⚡ Директория файла")]
        [JsonIgnore]
        public string Dir {
            get =>
            string.Format("{0}/{1}",
                Type.ToString(),
                Convert.ToHexStringLower([Id.ToByteArray()[15]])
            );
        }
        [Description(@"⚡ Имя файла с расширением")]
        [JsonIgnore]
        public string PhysicalFileName {
            get =>
            string.Format("{0}{1}",
                Id,
                string.IsNullOrEmpty(Extension)
                    ? ""
                    : string.Format(".{0}", Extension)
            );
        }
        [Description(@"⚡ Путь к файлу  
            Полный URL: http(s)://{{api_base}}/Uploads/{{path}}")]
        public string Path {
            get => string.Format("{0}/{1}",
                Dir,
                PhysicalFileName
            );
        }
    }
}
