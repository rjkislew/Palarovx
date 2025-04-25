using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Server.Palaro2026;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var bearerScheme = await authenticationSchemeProvider.GetSchemeAsync("Bearer");
        if (bearerScheme == null) return;

        // Create security scheme once
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        // Initialize components if needed
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        // Add security scheme
        document.Components.SecuritySchemes["Bearer"] = securityScheme;

        // Create security requirement once
        var securityRequirement = new OpenApiSecurityRequirement
        {
            [securityScheme] = new List<string>()
        };

        // Apply to all operations more efficiently
        foreach (var pathItem in document.Paths.Values)
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(securityRequirement);
            }
        }
    }
}