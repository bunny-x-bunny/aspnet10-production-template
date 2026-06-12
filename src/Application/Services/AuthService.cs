using Application.DTO.Auth;
using Application.Helpers;
using Application.Services.Interfaces;
using Domain.Enum;
using Domain.Models;
using Domain.Models.User;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using static Application.Helpers.Validation;

namespace Application.Services {
  public class AuthService(
      TimeProvider time_provider,
      IOptionsMonitor<BearerTokenOptions> bearer_token_options,
      IEmailSender<AppUser> identity_email_sender,
      Infrastructure.EmailSender.IEmailSender email_sender,
      LinkGenerator link_generator,
      UserManager<AppUser> user_manager,
      SignInManager<AppUser> signin_manager,
      IUserStore<AppUser> user_store,
      IOptions<AppSettings> settings,
      IHttpContextAccessor http_context_accessor,
      IWebHostEnvironment environment
  ) : IAuthService {
    private static readonly EmailAddressAttribute email_address_attribute = new();
    private readonly TimeProvider time_provider = time_provider;
    private readonly IOptionsMonitor<BearerTokenOptions> bearer_token_options = bearer_token_options;
    private readonly IEmailSender<AppUser> identity_email_sender = identity_email_sender;
    private readonly Infrastructure.EmailSender.IEmailSender email_sender = email_sender;
    private readonly LinkGenerator link_generator = link_generator;
    private readonly UserManager<AppUser> user_manager = user_manager;
    private readonly SignInManager<AppUser> signin_manager = signin_manager;
    public required IUserStore<AppUser> user_store = user_store;
    private readonly IOptions<AppSettings> settings = settings;
    private readonly IHttpContextAccessor http_context_accessor = http_context_accessor;
    private readonly IWebHostEnvironment environment = environment;
    private readonly AppUserEmailSender app_user_email_sender = new() {
      email_sender = email_sender,
      link_generator = link_generator
    };

    public async Task<Results<Ok<Guid>, ValidationProblem>> RegisterUser(RegisterDTO registration) {
      if (!user_manager.SupportsUserEmail)
        throw new NotSupportedException($"Auth service requires a user store with email support.");
      if (http_context_accessor.HttpContext is not { } http_context)
        throw new NotSupportedException("Auth service requires http context");

      var email_store = (IUserEmailStore<AppUser>)user_store;
      var email = registration.Email;

      if (string.IsNullOrEmpty(email) || !email_address_attribute.IsValid(email)) {
        return CreateValidationProblem(IdentityResult.Failed(user_manager.ErrorDescriber.InvalidEmail(email)));
      }

      var user = new AppUser {
        FullName = registration.FullName,
        Role = Role.User,
        //PhoneNumber = registration.PhoneNumber
      };
      await user_store.SetUserNameAsync(user, email, CancellationToken.None);
      await email_store.SetEmailAsync(user, email, CancellationToken.None);
      var result = await user_manager.CreateAsync(user, registration.Password);

      if (!result.Succeeded)
        return CreateValidationProblem(result);

      if (!environment.IsDevelopment())
        await app_user_email_sender.SendConfirmationEmailAsync(user, user_manager, http_context, email);
      return TypedResults.Ok(user.Id);
    }

    public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(string userId, string code, string? changedEmail) {
      if (await user_manager.FindByIdAsync(userId) is not { } user) {
        // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
        return TypedResults.Unauthorized();
      }

      try {
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
      } catch (FormatException) {
        return TypedResults.Unauthorized();
      }

      IdentityResult result;

      if (string.IsNullOrEmpty(changedEmail)) {
        result = await user_manager.ConfirmEmailAsync(user, code);
      } else {
        // As with Identity UI, email and user name are one and the same. So when we update the email,
        // we need to update the user name.
        result = await user_manager.ChangeEmailAsync(user, changedEmail, code);

        if (result.Succeeded) {
          result = await user_manager.SetUserNameAsync(user, changedEmail);
        }
      }

      if (!result.Succeeded) {
        return TypedResults.Unauthorized();
      }

      return TypedResults.Text("Благодарим вас за подтверждение email!  \n" +
        "Вы можете теперь войти в систему.");
    }

    private CookieOptions? LoginCokkie(string? origin, DateTimeOffset? expires) {
      CookieOptions cookie = new() {
        HttpOnly = false,
        SameSite = SameSiteMode.Lax,
        Expires = expires,
        Secure = true,
        Domain = null
      };
      switch (environment) {
        case var _ when environment.IsDevelopment():
          cookie = new CookieOptions() {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Expires = expires
          }; break;
        case var _ when environment.IsStaging():
          if ((origin?.StartsWith("http://domain.internal") ?? false) || (origin?.StartsWith("http://api.domain.internal") ?? false))
            cookie = new CookieOptions() {
              HttpOnly = false,
              SameSite = SameSiteMode.Lax,
              Expires = expires,
              Domain = "domain.internal",
              Secure = false
            };
          else if (origin == "https://domain.dev" || origin == "https://api.domain.dev")
            cookie = new CookieOptions() {
              HttpOnly = false,
              SameSite = SameSiteMode.Lax,
              Secure = true,
              Expires = expires,
              Domain = "domain.dev"
            };
          break;
        case var _ when environment.IsProduction():
          if (origin == "https://domain.com" || origin == "https://api.domain.com")
            cookie = new CookieOptions() {
              HttpOnly = true,
              SameSite = SameSiteMode.Lax,
              Secure = true,
              Expires = expires,
              Domain = "domain.com"
            };
          break;
      }
      return cookie;
    }

    public async Task<Results<EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest login) {
      if (http_context_accessor.HttpContext is not { } http_context)
        throw new NotSupportedException("Auth service requires http context");
      var origin = http_context.Request.Headers.Origin.FirstOrDefault();

      var user = await user_manager.FindByNameAsync(login.Email);
      if (user == null) {
        return TypedResults.Problem(Microsoft.AspNetCore.Identity.SignInResult.Failed.ToString(), statusCode: StatusCodes.Status401Unauthorized);
      }
      var attempt = await signin_manager.CheckPasswordSignInAsync(user, login.Password, true);
      if (!attempt.Succeeded) {
        return TypedResults.Problem(attempt.ToString(), statusCode: StatusCodes.Status401Unauthorized);
      }
      /*Role role;
      if (!Enum.TryParse((await user_manager.GetRolesAsync(user)).First(), out role)) {
          return TypedResults.Problem(Microsoft.AspNetCore.Identity.SignInResult.NotAllowed.ToString(), statusCode: StatusCodes.Status401Unauthorized);
      }*/
      var token = GenerateAccessToken(user.Id, user.Role, user.FullName, settings);
      var serialized_token = new JwtSecurityTokenHandler().WriteToken(token);
      if (LoginCokkie(origin, token.ValidTo) is not { } cookie)
        return TypedResults.Problem($"Invalid request origin: \"{origin}\"", statusCode: StatusCodes.Status403Forbidden);

      http_context.Response.Cookies.Append($".AspNetCore.{IdentityConstants.ApplicationScheme}", serialized_token, cookie);
      return TypedResults.Empty;
    }

    public Results<EmptyHttpResult, ProblemHttpResult> Logout(ClaimsPrincipal user) {
      if (http_context_accessor.HttpContext is not { } http_context)
        throw new NotSupportedException("Auth service requires http context");
      var origin = http_context.Request.Headers.Origin.FirstOrDefault();
      if (LoginCokkie(origin, null) is not { } cookie)
        return TypedResults.Problem($"Invalid request origin: \"{origin}\"", statusCode: StatusCodes.Status403Forbidden);
      http_context.Response.Cookies.Delete($".AspNetCore.{IdentityConstants.ApplicationScheme}", cookie);
      return TypedResults.Empty;
    }

    public async Task<Ok> ResendConfirmationEmail(ResendConfirmationEmailRequest resendRequest) {
      if (http_context_accessor.HttpContext is not { } http_context)
        throw new NotSupportedException("Auth service requires http context");

      if (await user_manager.FindByEmailAsync(resendRequest.Email) is not { } user) {
        return TypedResults.Ok();
      }

      await app_user_email_sender.SendConfirmationEmailAsync(user, user_manager, http_context, resendRequest.Email);
      return TypedResults.Ok();
    }

    public async Task<Results<Ok, ValidationProblem>> ForgotPassword(ForgotPasswordRequest resetRequest) {
      var user = await user_manager.FindByEmailAsync(resetRequest.Email);

      if (user is not null && await user_manager.IsEmailConfirmedAsync(user)) {
        var code = await user_manager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        await identity_email_sender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
      }

      // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
      // returned a 400 for an invalid code given a valid user email.
      return TypedResults.Ok();
    }

    public async Task<Results<Ok, ValidationProblem>> ResetPassword(ResetPasswordRequest resetRequest) {
      var user = await user_manager.FindByEmailAsync(resetRequest.Email);

      if (user is null || !(await user_manager.IsEmailConfirmedAsync(user))) {
        // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
        // returned a 400 for an invalid code given a valid user email.
        return CreateValidationProblem(IdentityResult.Failed(user_manager.ErrorDescriber.InvalidToken()));
      }

      IdentityResult result;
      try {
        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
        result = await user_manager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
      } catch (FormatException) {
        result = IdentityResult.Failed(user_manager.ErrorDescriber.InvalidToken());
      }

      if (!result.Succeeded) {
        return CreateValidationProblem(result);
      }

      return TypedResults.Ok();
    }

    public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> ManageCredentials(ClaimsPrincipal principial, InfoRequest infoRequest) {
      if (http_context_accessor.HttpContext is not { } http_context)
        throw new NotSupportedException("Auth service requires http context");

      if (await user_manager.GetUserAsync(principial) is not { } user) {
        return TypedResults.NotFound();
      }

      if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !email_address_attribute.IsValid(infoRequest.NewEmail)) {
        return CreateValidationProblem(IdentityResult.Failed(user_manager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
      }

      if (!string.IsNullOrEmpty(infoRequest.NewPassword)) {
        if (string.IsNullOrEmpty(infoRequest.OldPassword)) {
          return CreateValidationProblem("OldPasswordRequired", "The old password is required to set a new password. If the old password is forgotten, use /Auth/reset_password.");
        }

        var changePasswordResult = await user_manager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
        if (!changePasswordResult.Succeeded) {
          return CreateValidationProblem(changePasswordResult);
        }
      }

      if (!string.IsNullOrEmpty(infoRequest.NewEmail)) {
        var email = await user_manager.GetEmailAsync(user);

        if (email != infoRequest.NewEmail) {
          await app_user_email_sender.SendConfirmationEmailAsync(user, user_manager, http_context, infoRequest.NewEmail, isChange: true);
        }
      }

      return TypedResults.Ok(await CreateInfoResponseAsync(user, user_manager));
    }

    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class {
      return new() {
        Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
        IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
      };
    }

    // Generating token based on user information
    private static JwtSecurityToken GenerateAccessToken(Guid user_id, Role user_role, string user_name, IOptions<AppSettings> settings) {
      // Create user claims
      var claims = new List<Claim>
        {
          new Claim(ClaimTypes.NameIdentifier, user_id.ToString()),
          new Claim(ClaimTypes.Role, user_role.ToString()),
          new Claim(ClaimTypes.Name, user_name)
        };

      // Create a JWT
      var token = new JwtSecurityToken(
          issuer: settings.Value.Authentication.JWTIssuer,
          audience: settings.Value.Authentication.JWTAudience,
          claims: claims,
          expires: DateTime.UtcNow.Add(TimeSpan.FromSeconds(settings.Value.Authentication.BearerTokenExpiration)),
          signingCredentials: new SigningCredentials(
              new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Authentication.JWTKey)),
              SecurityAlgorithms.HmacSha256
          )
      );

      return token;
    }
  }
}
