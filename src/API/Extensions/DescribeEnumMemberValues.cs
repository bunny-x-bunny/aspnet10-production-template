using Microsoft.OpenApi;
//using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace API.Extensions {
  public class DescribeEnumMemberValues : ISchemaFilter {
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context) {
      if (context.Type.IsEnum) {
        //schema.Enum.Clear();
        var values = Enum.GetValues(context.Type).Cast<object>();
        //foreach (var kv in ) {
        //    schema.Enum.Add(new OpenApiString($"{kv} = {GetEnumDescription((Enum)kv)}"));
        //}
        schema.Description = string.Join("<br>", values
            .Select(x => (x, GetEnumDescription((Enum)x)))
            .Where(x => !string.IsNullOrEmpty(x.Item2))
            .Select(x => $"`{x.x}` = {x.Item2}"));
      }
    }

    private static string? GetEnumDescription(Enum value) {
      var fi = value.GetType().GetField(value.ToString());

      if (fi == null) return value.ToString();
      var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

      return attributes.Length > 0 ? attributes[0].Description : null;
    }
  }
}
