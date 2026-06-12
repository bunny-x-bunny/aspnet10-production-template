using Domain.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Auth {
    [Description("The request type for the \"/register\" endpoint added by <see cref=\"IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi\"/>.")]
    public class RegisterDTO {
        [Description("The user's email address which acts as a user name.")]
        [EmailAddress]
        [Required]
        [MaxLength(255)]
        public required string Email { get; init; }

        [Description("The user's password.")]
        [Required]
        public required string Password { get; init; }

        [MaxLength(255)]
        [Description("ФИО")]
        [Required]
        public required string FullName { get; set; }
    }
}
