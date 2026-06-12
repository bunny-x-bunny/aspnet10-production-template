using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using MR.AspNetCore.Pagination;

namespace API.Integration.Infrastructure {
  [Collection(nameof(ApiCollection))]
  public abstract class ApiTestBase {
    protected readonly ApiFixture fixture;
    protected static readonly JsonSerializerOptions JsonOptions = new() {
      PropertyNamingPolicy = null,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      Converters = { new JsonStringEnumConverter() },
      // API strips null properties on write, but several DTOs mark nullable
      // fields `required` (e.g. RieltorHistory.Comment). Relax the required
      // check on the client so deserialization survives missing nullable props.
      TypeInfoResolver = new DefaultJsonTypeInfoResolver {
        Modifiers = {
          static info => {
            foreach (var prop in info.Properties) prop.IsRequired = false;
          }
        }
      }
    };

    protected const string AdminEmail = "admin@example.com";
    protected const string UserEmail = "user@example.com";
    protected const string SeedPassword = "password";

    protected ApiTestBase(ApiFixture fixture) {
      this.fixture = fixture;
    }

    protected HttpClient CreateClient() => fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions {
      HandleCookies = true,
      AllowAutoRedirect = false,
    });

    protected async Task<HttpClient> LoginAsync(string email = AdminEmail, string password = SeedPassword) {
      var client = CreateClient();
      var response = await client.PostAsJsonAsync("/Auth/login", new LoginRequest {
        Email = email,
        Password = password
      }, JsonOptions);
      response.EnsureSuccessStatusCode();
      return client;
    }

    protected Task<HttpClient> LoginAdminAsync() => LoginAsync(AdminEmail);
    protected Task<HttpClient> LoginUserAsync() => LoginAsync(UserEmail);

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) {
      response.EnsureSuccessStatusCode();
      var content = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
      return content ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name} from response.");
    }

    protected static async Task<T> FirstPageItemAsync<T>(HttpResponseMessage response) {
      var page = await ReadAsync<KeysetPaginationResult<T>>(response);
      if (page.Data.Count == 0)
        throw new InvalidOperationException($"Paginated result of {typeof(T).Name} is empty");
      return page.Data[0];
    }

    protected static StringContent JsonBody(object value)
      => new(JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8, "application/json");
  }
}
