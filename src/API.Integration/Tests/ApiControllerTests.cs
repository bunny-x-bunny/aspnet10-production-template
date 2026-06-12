using API.Controllers;
using API.Integration.Infrastructure;

namespace API.Integration.Tests {
  public class ApiControllerTests : ApiTestBase {
    public ApiControllerTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetApiVersion_returns_version_info() {
      var client = CreateClient();
      var response = await client.GetAsync("/");
      var info = await ReadAsync<ApiController.VersionInfo>(response);

      Assert.False(string.IsNullOrEmpty(info.Version));
      Assert.NotEqual(default, info.BuildDate);
    }
  }
}
