using Domain.Models.User;

namespace Application.DTO.User {
    public class CreateAdminDTO : CreateUserDTO {

        public new AppUser ToEntity() => new() {
            Role = Domain.Enum.Role.Admin,

            PhoneNumber = PhoneNumber,
            FullName = FullName,
        };
    }
}
