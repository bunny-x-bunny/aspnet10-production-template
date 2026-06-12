using Application.DTO.Auth;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService service) : ControllerBase {
        private readonly IAuthService service = service;

        [HttpPost("register/user")]
        [EndpointDescription("Self-registration")]
        public async Task<Results<Ok<Guid>, ValidationProblem>> Register([FromBody] RegisterDTO registration)
            => await service.RegisterUser(registration);

        [HttpGet("confirm_email", Name = "/Auth/confirm_email")]
        [EndpointDescription("Email confirmation")]
        public async Task<Results<ContentHttpResult, UnauthorizedHttpResult>> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string code,
            [FromQuery] string? changedEmail
        ) => await service.ConfirmEmail(userId, code, changedEmail);

        [HttpPost("login")]
        [EndpointDescription("Login")]
        public async Task<Results<EmptyHttpResult, ProblemHttpResult>> Login([FromBody] LoginRequest login)
            => await service.Login(login);

        [HttpPost("logout")]
        [EndpointDescription("Logout")]
        [Authorize]
        public Results<EmptyHttpResult, ProblemHttpResult> Logout()
            => service.Logout(User);

        [HttpPost("resend_confirmation_email")]
        [EndpointDescription("Resend email confirmation code")]
        public async Task<Ok> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
            => await service.ResendConfirmationEmail(request);

        [HttpPost("forgot_password")]
        [EndpointDescription("Send password reset code via email")]
        public async Task<Results<Ok, ValidationProblem>> ForgotPassword([FromBody] ForgotPasswordRequest request)
            => await service.ForgotPassword(request);

        [HttpPost("reset_password")]
        [EndpointDescription("Reset password using a code")]
        public async Task<Results<Ok, ValidationProblem>> ResetPassword([FromBody] ResetPasswordRequest request)
            => await service.ResetPassword(request);

        [HttpPatch("manage/credentials")]
        [EndpointDescription("Change email and/or password")]
        [Authorize]
        public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> ManageCredentials([FromBody] InfoRequest request)
            => await service.ManageCredentials(User, request);
    }
}
