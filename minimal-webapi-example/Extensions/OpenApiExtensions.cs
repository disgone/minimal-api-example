using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace MinimumApiExample.Extensions;

internal static class OpenApiExtensions
{
    private static readonly OpenApiSecurityScheme JwtBearerScheme = new()
    {
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Name = JwtBearerDefaults.AuthenticationScheme,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    /// <summary>
    /// Adds JWT security metadata to the OpenAPI document for endpoints which require authorization.
    /// </summary>
    internal static IEndpointConventionBuilder AddOpenApiSecurityRequirement(this IEndpointConventionBuilder builder)
    {
        return AddRequiredSecurityMetadata(builder, operation => new(operation)
        {
            Security =
            {
                new()
                {
                    [JwtBearerScheme] = new List<string>()
                }
            }
        });
    }

    private static TBuilder AddRequiredSecurityMetadata<TBuilder>(TBuilder builder, Func<OpenApiOperation, OpenApiOperation> configureOperation)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Finally(endpointBuilder => AddAndConfigureOperationForEndpoint(endpointBuilder, configureOperation));
        return builder;
    }

    private static void AddAndConfigureOperationForEndpoint(EndpointBuilder endpointBuilder, Func<OpenApiOperation, OpenApiOperation> configure)
    {
        foreach (var item in endpointBuilder.Metadata)
        {
            if (item is not OpenApiOperation existingOperation)
            {
                continue;
            }

            // Only add security metadata for endpoints which require auth.
            if (!EndpointRequiresAuthentication(endpointBuilder))
            {
                return;
            }
                
            var configuredOperation = configure(existingOperation);

            if (ReferenceEquals(configuredOperation, existingOperation))
            {
                return;
            }

            endpointBuilder.Metadata.Remove(existingOperation);
            endpointBuilder.Metadata.Add(configuredOperation);

            return;
        }
    }

    private static bool EndpointRequiresAuthentication(EndpointBuilder endpointBuilder)
    {
        // This can have both authorization and anonymous metadata
        var authorizationAttr = endpointBuilder.Metadata.Where(r => r is IAllowAnonymous or IAuthorizeData).ToList();

        bool requiresAuthentication = false;

        foreach (var attr in authorizationAttr)
        {
            switch (attr)
            {
                // The presence of a anonymous attribute seems to override any auth attributes, so we can quit looking.
                case IAllowAnonymous:
                    return false;
                // We have an authorization attribute, but we need to check the rest.
                case IAuthorizeData:
                    requiresAuthentication = true;
                    break;
            }
        }

        return requiresAuthentication;
    }
}