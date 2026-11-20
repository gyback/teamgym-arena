using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeamGym.Arena.Api.OpenApi;

/// <summary>
/// The OpenAPI 3.1 contract of the API (ADR-0005). The build exports it to <c>src/Api/openapi.json</c>, and
/// <c>openapi-typescript</c> generates the SPA's types from that file, so every change here changes the contract.
/// </summary>
internal static class OpenApiConventions
{
    /// <summary>The name of the only OpenAPI document, served at <c>/openapi/v1.json</c> in development.</summary>
    public const string DocumentName = "v1";

    /// <summary>The component schema for RFC 9457 problem details.</summary>
    public const string ProblemDetailsSchemaId = nameof(ProblemDetails);

    /// <summary>The component schema for RFC 9457 problem details with field-level validation errors.</summary>
    public const string ValidationProblemDetailsSchemaId = nameof(HttpValidationProblemDetails);

    private const string ProblemJsonContentType = "application/problem+json";

    /// <summary>Registers the OpenAPI document and the conventions that every endpoint's description follows.</summary>
    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddOpenApi(DocumentName, options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
            options.AddDocumentTransformer(DescribeDocumentAsync);
            options.AddOperationTransformer(AddProblemResponses);
        });
    }

    private static async Task DescribeDocumentAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "TeamGym Arena API",
            Version = DocumentName,
        };

        // The build has no server address, and a running host must describe the same contract as the build.
        document.Servers = [];

        // Every operation can fail with problem details (see AddProblemResponses), and the SPA's API client types
        // its errors with these schemas even before an endpoint declares one itself.
        await AddSchemaIfMissingAsync<ProblemDetails>(document, context, ProblemDetailsSchemaId, cancellationToken)
            .ConfigureAwait(false);
        await AddSchemaIfMissingAsync<HttpValidationProblemDetails>(
                document, context, ValidationProblemDetailsSchemaId, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task AddSchemaIfMissingAsync<T>(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        string schemaId,
        CancellationToken cancellationToken)
    {
        if (document.Components?.Schemas?.ContainsKey(schemaId) == true)
        {
            return;
        }

        var schema = await context.GetOrCreateSchemaAsync(typeof(T), parameterDescription: null, cancellationToken)
            .ConfigureAwait(false);
        document.AddComponent(schemaId, schema);
    }

    // Responses that any endpoint can return without declaring them: the exception handler turns unhandled
    // exceptions into 500 problem details, and an endpoint that requires a user returns 401 without one. Declaring
    // them gives the generated client a typed error for every call.
    private static Task AddProblemResponses(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresUser = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();

        operation.Responses ??= [];

        if (requiresUser)
        {
            AddProblemResponse(operation, context, StatusCodes.Status401Unauthorized, "Unauthorized");
        }

        AddProblemResponse(operation, context, StatusCodes.Status500InternalServerError, "Internal Server Error");

        return Task.CompletedTask;
    }

    private static void AddProblemResponse(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        int statusCode,
        string description)
    {
        var key = statusCode.ToString(CultureInfo.InvariantCulture);
        if (operation.Responses!.ContainsKey(key))
        {
            return;
        }

        operation.Responses[key] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [ProblemJsonContentType] = new()
                {
                    Schema = new OpenApiSchemaReference(ProblemDetailsSchemaId, context.Document),
                },
            },
        };
    }
}
