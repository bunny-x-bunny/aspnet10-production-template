using System.Net;
using System.Net.Http.Json;
using API.Integration.Infrastructure;
using Application.DTO.User;
using Domain.Enum;
using MR.AspNetCore.Pagination;

namespace API.Integration.Tests {
  public class UserControllerTests : ApiTestBase {
    public UserControllerTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetAll_returns_users() {
      var client = await LoginAdminAsync();
      var page = await ReadAsync<KeysetPaginationResult<GetUserExtDTO>>(
        await client.GetAsync("/User"));
      Assert.True(page.Data.Count >= 2);
    }

    [Fact]
    public async Task GetSelf_returns_current_admin() {
      var client = await LoginAdminAsync();
      var self = await ReadAsync<GetUserExtDTO>(await client.GetAsync("/User/self"));
      Assert.Equal(AdminEmail, self.Email);
      Assert.Equal(Role.Admin, self.Role);
    }

    [Fact]
    public async Task UpdateSelfBasic_changes_full_name() {
      var client = await LoginUserAsync();
      var response = await client.PutAsJsonAsync("/User/self/basic", new UserBasicDTO {
        FullName = "Updated Self Name",
        PhoneNumber = "+996555000111"
      }, JsonOptions);
      var updated = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal("Updated Self Name", updated.FullName);
    }

    [Fact]
    public async Task GetById_returns_seeded_admin() {
      var client = await LoginAdminAsync();
      var self = await ReadAsync<GetUserExtDTO>(await client.GetAsync("/User/self"));

      var user = await ReadAsync<GetUserExtDTO>(await client.GetAsync($"/User/{self.Id}"));
      Assert.Equal(self.Id, user.Id);
    }

    [Fact]
    public async Task CreateUser_admin_creates_new_rieltor() {
      var client = await LoginAdminAsync();
      var email = $"u-{Guid.NewGuid():N}@example.com";

      var response = await client.PostAsJsonAsync("/User/User", new CreateUserDTO {
        Email = email,
        Password = "password",
        FullName = "Created User",
      }, JsonOptions);

      var created = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal(email, created.Email);
      Assert.Equal(Role.User, created.Role);
    }

    [Fact]
    public async Task CreateAdmin_admin_creates_new_admin() {
      var client = await LoginAdminAsync();
      var email = $"a-{Guid.NewGuid():N}@example.com";

      var response = await client.PostAsJsonAsync("/User/Admin", new CreateAdminDTO {
        Email = email,
        Password = "password",
        FullName = "Created Admin",
      }, JsonOptions);

      var created = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal(email, created.Email);
      Assert.Equal(Role.Admin, created.Role);
    }

    [Fact]
    public async Task UpdateUser_admin_changes_user_fullname() {
      var client = await LoginAdminAsync();
      var created = await CreateUser(client, "User");

      var response = await client.PutAsJsonAsync($"/User/User/{created.Id}", new UpdateUserDTO {
        FullName = "Renamed User",
        PhoneNumber = null,
      }, JsonOptions);

      var updated = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal("Renamed User", updated.FullName);
    }

    [Fact]
    public async Task UpdateAdmin_admin_changes_admin_fullname() {
      var client = await LoginAdminAsync();
      var created = await CreateUser(client, "Admin");

      var response = await client.PutAsJsonAsync($"/User/Admin/{created.Id}", new UpdateUserDTO {
        FullName = "Renamed Admin",
        PhoneNumber = null,
      }, JsonOptions);

      var updated = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal("Renamed Admin", updated.FullName);
    }

    [Fact]
    public async Task UpdateCredentials_admin_changes_email() {
      var client = await LoginAdminAsync();
      var created = await CreateUser(client, "User");
      var newEmail = $"creds-{Guid.NewGuid():N}@example.com";

      var response = await client.PatchAsJsonAsync($"/User/{created.Id}/credentials",
        new UpdateUserCredentialsDTO { Email = newEmail }, JsonOptions);
      var updated = await ReadAsync<GetUserBasicDTO>(response);
      Assert.Equal(newEmail, updated.Email);
    }

    [Fact]
    public async Task Delete_admin_removes_user() {
      var client = await LoginAdminAsync();
      var created = await CreateUser(client, "User");

      var response = await client.DeleteAsync($"/User/{created.Id}?delete_files=false");
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      var notFound = await client.GetAsync($"/User/{created.Id}");
      Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    private async Task<GetUserBasicDTO> CreateUser(HttpClient client, string kind) {
      var email = $"x-{Guid.NewGuid():N}@example.com";
      var body = new CreateUserDTO {
        Email = email,
        Password = "password",
        FullName = $"Created {kind}",
      };
      var response = await client.PostAsJsonAsync($"/User/{kind}", body, JsonOptions);
      return await ReadAsync<GetUserBasicDTO>(response);
    }
  }
}
