using Application.DTO.User;
using Application.Services.Interfaces;
using Common.Helpers;
using Domain.Enum;
using Domain.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static Application.Helpers.Validation;

namespace Application.Services {
  public class UserService(
      AppDbContext context,
      UserManager<AppUser> user_manager,
      IUserStore<AppUser> user_store,
      Infrastructure.EmailSender.IEmailSender email_sender,
      IFileService files
  ) : IUserService {
    private static readonly EmailAddressAttribute email_address_attribute = new();
    private readonly AppDbContext context = context;
    private readonly UserManager<AppUser> user_manager = user_manager;
    private readonly IUserStore<AppUser> user_store = user_store;
    private readonly Infrastructure.EmailSender.IEmailSender email_sender = email_sender;
    private readonly IFileService files = files;

    public IQueryable<AppUser> All() => context.Users
      .Include(u => u.Avatar);

    public IQueryable<GetUserExtDTO> Project(IQueryable<AppUser> query)
      => query
      .Select(u => GetUserExtDTO.FromEntity(
        u // entity
      ));

    public async Task<AppUser?> Find(Guid uuid) => await All().FirstOrDefaultAsync(u => u.Id == uuid);

    public async Task<Results<Ok<T>, ValidationProblem>> CreateUser<T>(T user, string email, string? password) where T : AppUser {
      if (!user_manager.SupportsUserEmail)
        throw new NotSupportedException($"Auth service requires a user store with email support.");

      // generate password if empty
      if (string.IsNullOrEmpty(password)) {
        password = SecureRandomStringGenerator.Generate(8);
      }

      var email_store = (IUserEmailStore<AppUser>)user_store;

      if (string.IsNullOrEmpty(email) || !email_address_attribute.IsValid(email))
        return CreateValidationProblem(IdentityResult.Failed(user_manager.ErrorDescriber.InvalidEmail(email)));

      await user_store.SetUserNameAsync(user, email, CancellationToken.None);
      await email_store.SetEmailAsync(user, email, CancellationToken.None);
      var result = await user_manager.CreateAsync(user, password);

      if (!result.Succeeded)
        return CreateValidationProblem(result);

      // send email with credentials
      await email_sender.SendEmailAsync(email, "Ваш аккаунт NewProject.com", $@"
        <b>Роль:</b> {user.Role}<br>
        <b>Email:</b> {email}<br>
        <b>Пароль:</b> <code>{password}</code>
      ", true);

      return TypedResults.Ok(user);
    }

    public async Task<Results<Ok<AppUser>, NotFound>> UpdateBasic(Guid id, UserBasicDTO request) {
      if (await context.Users.FindAsync(id) is not AppUser user) return TypedResults.NotFound();
      user = request.UpdateEntity(user);
      await context.SaveChangesAsync();
      return TypedResults.Ok(user);
    }

    /*public async Task UpdateLang(Guid uuid, string lang)
        => await context.Users
            .Where(u => u.Id == uuid)
            .ExecuteUpdateAsync(b => b.SetProperty(u => u.UiLang, lang));*/

    public async Task<Results<Ok<AppUser>, ValidationProblem, NotFound>> UpdateCredentials(Guid uuid, UpdateUserCredentialsDTO request) {
      if (!user_manager.SupportsUserEmail)
        throw new NotSupportedException($"Auth service requires a user store with email support.");
      var email_store = (IUserEmailStore<AppUser>)user_store;

      if (await context.Users.FindAsync(uuid) is not AppUser user) return TypedResults.NotFound();

      await using var transaction = await context.Database.BeginTransactionAsync();

      // handle email change
      if (request.Email is not null) {
        if (string.IsNullOrEmpty(request.Email) || !email_address_attribute.IsValid(request.Email))
          return CreateValidationProblem(IdentityResult.Failed(user_manager.ErrorDescriber.InvalidEmail(request.Email)));

        user.NormalizedEmail = request.Email.ToUpper();
        user.NormalizedUserName = request.Email.ToUpper();
        await user_store.SetUserNameAsync(user, request.Email, CancellationToken.None);
        await email_store.SetEmailAsync(user, request.Email, CancellationToken.None);
      }

      // handle password change
      if (request.Password is not null) {
        await user_manager.RemovePasswordAsync(user);
        var result = await user_manager.AddPasswordAsync(user, request.Password);
        if (!result.Succeeded)
          return CreateValidationProblem(result);
      }

      // handle role change
      if (request.Role is Role role)
        user.Role = role;

      await context.SaveChangesAsync();
      await transaction.CommitAsync();
      return TypedResults.Ok(user);
    }

    public async Task<Results<Ok<AppUser>, NotFound>> UpdateAdmin(Guid id, UpdateUserDTO request) {
      if (await context.Users.FindAsync(id) is not AppUser user) return TypedResults.NotFound();
      user = request.UpdateEntity(user);
      await context.SaveChangesAsync();
      return TypedResults.Ok(user);
    }

    public async Task<Results<Ok<AppUser>, NotFound>> UpdateUser(Guid id, UpdateUserDTO request) {
      if (await context.Users.FindAsync(id) is not AppUser user) return TypedResults.NotFound();
      user = request.UpdateEntity(user);
      await context.SaveChangesAsync();
      return TypedResults.Ok(user);
    }

    public async Task<Results<Ok<AppUser>, ValidationProblem>> UpdateAvatar(AppUser user, Domain.Models.File? file) {
      user.AvatarId = file?.Id;
      user.Avatar = file;
      await context.SaveChangesAsync();
      return TypedResults.Ok(user);
    }

    public async Task Delete(AppUser entity, bool delete_files) {
      if (delete_files) try {
        var user_files = await context.Files.Where(f => f.UserId == entity.Id).ToListAsync();
        foreach (var file in user_files) {
          await files.DeleteFile(file);
        }
      } catch (Exception e) { System.Diagnostics.Trace.TraceError(e.ToString()); }

      context.Users.Remove(entity);
      await context.SaveChangesAsync();
    }

    /*public async Task<ISet<Permission>> UpdatePermissions(AppUser user, ISet<Permission> request) {
      user.Permissions = request;
      await context.SaveChangesAsync();
      return request;
    }*/
  }
}
