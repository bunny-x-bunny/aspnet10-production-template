using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using Swashbuckle.AspNetCore.Filters.Extensions;
using System.Reflection;

namespace API.Extensions {
    public class Swagger {
        public class AppendAuthorizeToSummaryOperationFilter : IOperationFilter {
            private readonly AppendAuthorizeToSummaryOperationFilter<AuthorizeAttribute> filter;

            public AppendAuthorizeToSummaryOperationFilter() {
                var policySelector = new PolicySelectorWithLabel<AuthorizeAttribute> {
                    Label = "policies",
                    Selector = authAttributes =>
                        authAttributes
                            .Where(a => !string.IsNullOrEmpty(a.Policy))
                            .Select(a => a.Policy)
                };

                var rolesSelector = new PolicySelectorWithLabel<AuthorizeAttribute> {
                    Label = "roles",
                    Selector = authAttributes =>
                        authAttributes
                            .Where(a => !string.IsNullOrEmpty(a.Roles))
                            .Select(a => a.Roles)
                };

                filter = new AppendAuthorizeToSummaryOperationFilter<AuthorizeAttribute>(new[] { policySelector, rolesSelector }.AsEnumerable());
            }

            public void Apply(OpenApiOperation operation, OperationFilterContext context) {
                filter.Apply(operation, context);
            }
        }

        public class AppendAuthorizeToSummaryOperationFilter<T> : IOperationFilter where T : Attribute {
            private readonly IEnumerable<PolicySelectorWithLabel<T>> policySelectors;

            /// <summary>
            /// Constructor for AppendAuthorizeToSummaryOperationFilter
            /// </summary>
            /// <param name="policySelectors">Used to select the authorization policy from the attribute e.g. (a => a.Policy)</param>
            public AppendAuthorizeToSummaryOperationFilter(IEnumerable<PolicySelectorWithLabel<T>> policySelectors) {
                this.policySelectors = policySelectors;
            }

            public void Apply(OpenApiOperation operation, OperationFilterContext context) {
                if (context.GetControllerAndActionAttributes<AllowAnonymousAttribute>().Any()) {
                    return;
                }
                /*var authorizeAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                    .Union(context.MethodInfo.GetCustomAttributes(true))
                    .OfType<T>();*/
                var authorizeAttributes = context.GetControllerAndActionAttributes<T>();

                if (authorizeAttributes.Any()) {
                    var authorizationDescription = new StringBuilder("<p><strong>🈲 Authorized");

                    foreach (var policySelector in policySelectors) {
                        AppendPolicies(authorizeAttributes, authorizationDescription, policySelector);
                    }
                    operation.Description += authorizationDescription.ToString().TrimEnd(';') + "\n<strong></p>";
                }
            }

            private void AppendPolicies(IEnumerable<T> authorizeAttributes, StringBuilder authorizationDescription, PolicySelectorWithLabel<T> policySelector) {
                var policies = policySelector.Selector(authorizeAttributes)
                    .OrderBy(policy => policy);

                if (policies.Any()) {
                    authorizationDescription.Append($" {policySelector.Label}: {string.Join(", ", policies)};");
                }
            }
        }
    }
    internal static class OperationFilterContextExtensions {
        public static IEnumerable<T> GetControllerAndActionAttributes<T>(this OperationFilterContext context) where T : Attribute {
            var result = new List<T>();

            if (context.MethodInfo != null) {
                if (context.MethodInfo.ReflectedType?.GetTypeInfo().GetCustomAttributes<T>() is { } controllerAttributes)
                    result.AddRange(controllerAttributes);

                var actionAttributes = context.MethodInfo.GetCustomAttributes<T>();
                result.AddRange(actionAttributes);
            }

#if NETCOREAPP3_1_OR_GREATER
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata != null) {
                var endpointAttributes = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<T>();
                result.AddRange(endpointAttributes);
            }
#endif
            return result.Distinct();
        }
    }
}
