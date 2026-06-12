using System.Net;
using System.Net.Http.Json;
using API.Integration.Infrastructure;
using Application.DTO.Notification;
using Application.DTO.User;
using Domain.Enum;
using MR.AspNetCore.Pagination;

namespace API.Integration.Tests {
  public class NotificationControllerTests : ApiTestBase {
    public NotificationControllerTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetMy_returns_list_even_when_empty() {
      var client = await LoginAdminAsync();
      var page = await ReadAsync<KeysetPaginationResult<GetNotificationDTO>>(
        await client.GetAsync("/Notification/my"));
      Assert.NotNull(page.Data);
    }

    [Fact]
    public async Task ConfirmRead_returns_ok_for_empty_list() {
      var client = await LoginAdminAsync();
      var response = await client.PostAsJsonAsync("/Notification/read/true",
        Array.Empty<Guid>(), JsonOptions);
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
  }
}
