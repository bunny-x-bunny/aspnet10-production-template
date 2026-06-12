using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.User {
    public class UserBasicDTO {
        [Description("Телефон")]
        [Phone]
        [MaxLength(63)]
        public string? PhoneNumber { get; set; }
        [MaxLength(255)]
        [Description("ФИО")]
        [Required]
        public required string FullName { get; set; }

        public T UpdateEntity<T>(T entity) where T: AppUser {
            entity.PhoneNumber = PhoneNumber;
            entity.FullName = FullName;
            return entity;
        }
    }
}
