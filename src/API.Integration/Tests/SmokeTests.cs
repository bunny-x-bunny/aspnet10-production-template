using API.Integration.Infrastructure;

namespace API.Integration.Tests {
  public class SmokeTests : ApiTestBase {
    public SmokeTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Container_starts_and_seed_runs() {
      var client = CreateClient();
      var response = await client.GetAsync("/");
      response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Admin_can_login() {
      var client = await LoginAdminAsync();
      Assert.NotNull(client);
    }

    [Fact]
    public async Task User_can_login() {
      var client = await LoginUserAsync();
      Assert.NotNull(client);
    }
  }
}
