using Testcontainers.PostgreSql;

namespace API.Integration.Infrastructure {
  public class ApiFixture : IAsyncLifetime {
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("postgres:17.4-alpine3.21")
      .WithDatabase("newproject_test")
      .WithUsername("postgres")
      .WithPassword("postgres")
      .Build();

    public TestApiFactory Factory { get; private set; } = null!;
    public string UploadPath { get; private set; } = null!;

    public async Task InitializeAsync() {
      await container.StartAsync();

      UploadPath = Path.Combine(Path.GetTempPath(), "newproject-tests-" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(UploadPath);

      Factory = new TestApiFactory(container.GetConnectionString(), UploadPath);
      await Factory.ApplyMigrationsAsync();

      // Touching CreateClient triggers Program.cs startup → seed populates the schema.
      using var _ = Factory.CreateClient();
    }

    public async Task DisposeAsync() {
      await Factory.DisposeAsync();
      await container.DisposeAsync();

      try {
        if (Directory.Exists(UploadPath))
          Directory.Delete(UploadPath, recursive: true);
      } catch { /* best-effort cleanup */ }
    }
  }

  [CollectionDefinition(nameof(ApiCollection))]
  public class ApiCollection : ICollectionFixture<ApiFixture> { }
}
