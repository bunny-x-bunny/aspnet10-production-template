using Microsoft.AspNetCore.Http;

namespace Application.Extensions {
    public static class HttpContrxtExt {
        public static string Lang(this IHttpContextAccessor context) {
            var lang = context.HttpContext?.Request.Headers["X-LANG"];
            return !string.IsNullOrEmpty(lang)
                ? lang!
                : "en";
        }

        public static string? BaseUrl(this IHttpContextAccessor context) =>
            context.HttpContext is not null
                ? $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}"
                : null;
    }
}
