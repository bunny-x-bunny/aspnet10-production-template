using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
  [Route("[controller]")]
  [ApiController]
  public class ApiController : ControllerBase {
    public class VersionInfo {
      public required string Version { get; set; }
      public required DateTime BuildDate { get; set; }
    }


    [HttpGet("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public VersionInfo GetApiVersion() {
      var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      return new VersionInfo {
        Version = version?.ToString()!,
        BuildDate = new DateTime(2000, 1, 1).AddDays(version!.Build).AddSeconds(version.Revision * 2)
      };
    }
  }
}
