using System.Net;
using System.Net.Http.Json;
using API.Integration.Infrastructure;
using Application.DTO.Cat;
using MR.AspNetCore.Pagination;

namespace API.Integration.Tests {
  public class CatControllerTests : ApiTestBase {
    public CatControllerTests(ApiFixture fixture) : base(fixture) { }

    // ---------- reads (public) ----------

    [Fact]
    public async Task GetAll_returns_cats() {
      var client = CreateClient();
      var page = await ReadAsync<KeysetPaginationResult<GetCatDTO>>(
        await client.GetAsync("/Cat"));
      Assert.NotEmpty(page.Data);
    }

    [Fact]
    public async Task GetFamily_returns_seeded_root() {
      var client = CreateClient();
      var roots = await ReadAsync<List<GetCatFamilyEntry>>(
        await client.GetAsync("/Cat/family"));
      Assert.Contains(roots, r => r.Name == "root");
    }

    [Fact]
    public async Task Get_unknown_returns_not_found() {
      var client = CreateClient();
      var response = await client.GetAsync($"/Cat/{Guid.NewGuid()}");
      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_returns_created_cat() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);
      var created = await CreateChildAsync(client, root.Id, "get-target");

      var fetched = await ReadAsync<GetCatDTO>(await client.GetAsync($"/Cat/{created.Id}"));
      Assert.Equal(created.Id, fetched.Id);
      Assert.Equal("get-target", fetched.Name);
    }

    // ---------- writes (admin) ----------

    [Fact]
    public async Task Create_admin_adds_child_under_root() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);

      var created = await CreateChildAsync(client, root.Id, "child-cat");
      Assert.Equal(root.Id, created.ParentId);
      Assert.Equal("child-cat", created.Name);
      // fresh node is a leaf: nested-set width is exactly 1
      Assert.Equal(1, created.RightEar - created.LeftEar);
    }

    [Fact]
    public async Task Create_unknown_parent_returns_not_found() {
      var client = await LoginAdminAsync();
      var response = await client.PostAsJsonAsync($"/Cat/{Guid.NewGuid()}",
        new UpdateCat { Name = "orphan" }, JsonOptions);
      Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_admin_renames_cat() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);
      var created = await CreateChildAsync(client, root.Id, "before");

      var updated = await ReadAsync<GetCatDTO>(
        await client.PutAsJsonAsync($"/Cat/{created.Id}",
          new UpdateCat { Name = "after" }, JsonOptions));
      Assert.Equal(created.Id, updated.Id);
      Assert.Equal("after", updated.Name);
    }

    [Fact]
    public async Task Delete_admin_removes_leaf_cat() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);
      var created = await CreateChildAsync(client, root.Id, "doomed");

      var response = await client.DeleteAsync($"/Cat/{created.Id}");
      Assert.Equal(HttpStatusCode.OK, response.StatusCode);

      var notFound = await client.GetAsync($"/Cat/{created.Id}");
      Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Delete_root_rejected_with_validation_problem() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);

      // deleting a root cat is forbidden by the service (NotAChildCat)
      var response = await client.DeleteAsync($"/Cat/{root.Id}");
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBreadcrumbs_returns_path_root_to_cat() {
      var client = await LoginAdminAsync();
      var root = await GetRootAsync(client);
      var created = await CreateChildAsync(client, root.Id, "leaf");

      var path = await ReadAsync<List<GetCatBasic>>(
        await client.GetAsync($"/Cat/breadcrumbs?cat_id={created.Id}"));
      Assert.NotEmpty(path);
      Assert.Equal(root.Id, path[0].Id);
      Assert.Equal(created.Id, path[^1].Id);
    }

    // ---------- authorization ----------

    [Fact]
    public async Task Create_requires_authentication() {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync($"/Cat/{Guid.NewGuid()}",
        new UpdateCat { Name = "nope" }, JsonOptions);
      Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_forbidden_for_non_admin() {
      var client = await LoginUserAsync();
      var root = await GetRootAsync(client);
      var response = await client.PostAsJsonAsync($"/Cat/{root.Id}",
        new UpdateCat { Name = "nope" }, JsonOptions);
      Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------- helpers ----------

    private async Task<GetCatFamilyEntry> GetRootAsync(HttpClient client) {
      var roots = await ReadAsync<List<GetCatFamilyEntry>>(
        await client.GetAsync("/Cat/family"));
      return roots.First(r => r.Name == "root");
    }

    private async Task<GetCatDTO> CreateChildAsync(HttpClient client, Guid parentId, string name)
      => await ReadAsync<GetCatDTO>(
        await client.PostAsJsonAsync($"/Cat/{parentId}", new UpdateCat { Name = name }, JsonOptions));
  }
}
