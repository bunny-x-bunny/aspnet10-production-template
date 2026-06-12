using Application.DTO.User;
using Domain.Enum;
using Domain.Models.User;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services.Interfaces {
  public interface IUserService {
    IQueryable<AppUser> All();
    Task<Results<Ok<T>, ValidationProblem>> CreateUser<T>(T user, string email, string password) where T : AppUser;
    Task Delete(AppUser entity, bool delete_files);
    Task<AppUser?> Find(Guid uuid);
    IQueryable<GetUserExtDTO> Project(IQueryable<AppUser> query);
    Task<Results<Ok<AppUser>, NotFound>> UpdateAdmin(Guid id, UpdateUserDTO request);
    Task<Results<Ok<AppUser>, ValidationProblem>> UpdateAvatar(AppUser user, Domain.Models.File? file);
    Task<Results<Ok<AppUser>, NotFound>> UpdateBasic(Guid id, UserBasicDTO request);
    Task<Results<Ok<AppUser>, ValidationProblem, NotFound>> UpdateCredentials(Guid uuid, UpdateUserCredentialsDTO request);
    Task<Results<Ok<AppUser>, NotFound>> UpdateUser(Guid id, UpdateUserDTO request);
  }
}