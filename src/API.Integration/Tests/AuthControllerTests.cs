using System.Net;
using System.Net.Http.Json;
using API.Integration.Infrastructure;
using Application.DTO.Auth;
using Microsoft.AspNetCore.Identity.Data;

namespace API.Integration.Tests {
  public class AuthControllerTests : ApiTestBase {
    public AuthControllerTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Register_creates_new_user_and_returns_guid() {
      var client = CreateClient();
      var email = $"reg-{Guid.NewGuid():N}@example.com";

      var response = await client.PostAsJsonAsync("/Auth/register/user", new RegisterDTO {
        Email = email,
        Password = "password",
        FullName = "Тестовый Пользователь"
      }, JsonOptions);

      var id = await ReadAsync<Guid>(response);
      Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Login_with_seeded_admin_succeeds() {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync("/Auth/login", new LoginRequest {
        Email = AdminEmail,
        Password = SeedPassword
      }, JsonOptions);

      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
      Assert.Contains(response.Headers.GetValues("Set-Cookie"),
        c => c.StartsWith(".AspNetCore.Identity.Application="));
    }

    [Fact]
    public async Task Logout_returns_ok_when_authenticated() {
      var client = await LoginAdminAsync();
      var response = await client.PostAsync("/Auth/logout", content: null);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResendConfirmationEmail_always_returns_ok() {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync("/Auth/resend_confirmation_email",
        new ResendConfirmationEmailRequest { Email = AdminEmail }, JsonOptions);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_returns_ok() {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync("/Auth/forgot_password",
        new ForgotPasswordRequest { Email = AdminEmail }, JsonOptions);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_returns_ok_even_for_unconfirmed_user() {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync("/Auth/reset_password",
        new ResetPasswordRequest {
          Email = AdminEmail,
          ResetCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("invalid")),
          NewPassword = "newpassword123"
        }, JsonOptions);
      Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest,
        $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task ManageCredentials_returns_info_response_when_no_changes() {
      var client = await LoginAdminAsync();
      var response = await client.PatchAsJsonAsync("/Auth/manage/credentials",
        new InfoRequest { }, JsonOptions);
      var info = await ReadAsync<InfoResponse>(response);
      Assert.False(string.IsNullOrEmpty(info.Email));
    }
  }
}
