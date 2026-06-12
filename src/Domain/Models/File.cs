using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Models {
    public class File {
        [Key]
        public Guid Id { get; init; }
        [Description("Type")]
        public required FileType Type { get; set; }
        [Description("File creator")]
        public Guid? UserId { get; set; }
        public AppUser? User { get; set; } = null!;
        [Description("Name without extension")]
        [MaxLength(255)]
        public string? Name { get; set; }
        [Description("Extension")]
        public required string Extension { get; set; }
        [Description("Size")]
        public required long Size { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Description(@"⚡ File directory")]
        [JsonIgnore]
        public string Dir {
            get =>
            string.Format("{0}/{1}",
                Type.ToString(),
                Convert.ToHexStringLower([Id.ToByteArray()[15]])
            );
        }
        [Description(@"⚡ File name with extension")]
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
        [Description(@"⚡ File path
            Full URL: http(s)://{{api_base}}/Uploads/{{path}}")]
        public string Path {
            get => string.Format("{0}/{1}",
                Dir,
                PhysicalFileName
            );
        }
    }
}
