using Application.DTO.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Claims;

namespace Application.Services.Interfaces {
    public interface IAuthService {
        Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(string userId, string code, string? changedEmail);
        Task<Results<Ok, ValidationProblem>> ForgotPassword(ForgotPasswordRequest resetRequest);
        Task<Results<EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest login);
        Results<EmptyHttpResult, ProblemHttpResult> Logout(ClaimsPrincipal user);
        Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> ManageCredentials(ClaimsPrincipal principial, InfoRequest infoRequest);
        Task<Results<Ok<Guid>, ValidationProblem>> RegisterUser(RegisterDTO registration);
        Task<Ok> ResendConfirmationEmail(ResendConfirmationEmailRequest resendRequest);
        Task<Results<Ok, ValidationProblem>> ResetPassword(ResetPasswordRequest resetRequest);
    }
}