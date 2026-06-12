using System.Net;
using System.Net.Http.Headers;
using API.Integration.Infrastructure;
using Application.DTO.File;
using MR.AspNetCore.Pagination;

namespace API.Integration.Tests {
  public class FileControllerTests : ApiTestBase {
    public FileControllerTests(ApiFixture fixture) : base(fixture) { }

    // Minimal 1x1 PNG (67 bytes), valid bytes — used as test content.
    private static readonly byte[] MinimalPng = [
      0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
      0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
      0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
      0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
      0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41,
      0x54, 0x08, 0x99, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
      0x00, 0x00, 0x03, 0x00, 0x01, 0x5B, 0xB1, 0x68,
      0xC1, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E,
      0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    [Fact]
    public async Task GetAll_returns_files_list_for_user() {
      var client = await LoginUserAsync();
      var page = await ReadAsync<KeysetPaginationResult<GetFileDTO>>(
        await client.GetAsync("/File"));
      Assert.NotNull(page.Data);
    }

    [Fact]
    public async Task Upload_estate_image_returns_file_dto() {
      var client = await LoginAdminAsync();
      using var content = BuildMultipart(MinimalPng, "test.png", "image/png");

      var file = await ReadAsync<GetFileDTO>(await client.PostAsync("/File/EstateImage", content));
      Assert.NotEqual(Guid.Empty, file.Id);
      Assert.Equal("png", file.Extension);
    }

    [Fact]
    public async Task UpdateAvatar_sets_and_returns_avatar_dto() {
      var client = await LoginAdminAsync();
      using var content = BuildMultipart(MinimalPng, "avatar.png", "image/png");

      var avatar = await ReadAsync<GetFileDTO>(await client.PutAsync("/File/self/avatar", content));
      Assert.NotEqual(Guid.Empty, avatar.Id);
    }

    [Fact]
    public async Task Delete_removes_uploaded_file() {
      var client = await LoginAdminAsync();

      using var uploadContent = BuildMultipart(MinimalPng, "to-delete.png", "image/png");
      var uploaded = await ReadAsync<GetFileDTO>(
        await client.PostAsync("/File/EstateImage", uploadContent));

      var deleted = await client.DeleteAsync($"/File/{uploaded.Id}");
      Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
    }

    private static MultipartFormDataContent BuildMultipart(byte[] bytes, string filename, string contentType) {
      var content = new MultipartFormDataContent();
      var fileContent = new ByteArrayContent(bytes);
      fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
      content.Add(fileContent, "file", filename);
      return content;
    }
  }
}
