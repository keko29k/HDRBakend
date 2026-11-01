using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(x => x.Name == "Bearer"))
        {
            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes =
                new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    }
                };

            // Agregar la seguridad a las operaciones documentadas
            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];

                    operation.Security.Add(
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference(
                                "Bearer",
                                document
                            )] = []
                        }
                    );
                }
            }
        }
    }
}