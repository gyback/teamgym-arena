using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TeamGym.Arena.Api.OpenApi;

namespace TeamGym.Arena.Api.IntegrationTests;

public sealed class OpenApiContractTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string DocumentPath = "/openapi/v1.json";
    private const string ProblemDetailsRef = "#/components/schemas/ProblemDetails";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task DocumentDefinesTheProblemDetailsSchemasEvenWithoutEndpointsThatUseThem()
    {
        using var client = factory.CreateClient();
        using var document = await GetDocumentAsync(client);

        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty(OpenApiConventions.ProblemDetailsSchemaId, out _));
        var validationProblem = schemas.GetProperty(OpenApiConventions.ValidationProblemDetailsSchemaId);
        Assert.True(validationProblem.GetProperty("properties").TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task DocumentDescribesNumbersAsJsonNumbersOnly()
    {
        using var client = factory.CreateClient();
        using var document = await GetDocumentAsync(client);

        var status = document.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty(OpenApiConventions.ProblemDetailsSchemaId).GetProperty("properties").GetProperty("status");
        var types = status.GetProperty("type").EnumerateArray().Select(type => type.GetString()).ToList();
        Assert.Equal(["null", "integer"], types);
    }

    [Fact]
    public async Task DocumentDoesNotDependOnTheServerItIsServedFrom()
    {
        using var client = factory.CreateClient();
        using var document = await GetDocumentAsync(client);

        Assert.False(
            document.RootElement.TryGetProperty("servers", out var servers) && servers.GetArrayLength() > 0,
            "The build-time export has no server address, so the served document must not have one either.");
    }

    // Generated client methods and TanStack Query keys are named after operation IDs. Name every endpoint with
    // WithName().
    [Fact]
    public async Task EveryOperationHasAUniqueOperationId()
    {
        using var client = factory.CreateClient();
        using var document = await GetDocumentAsync(client);

        var operations = Operations(document).ToList();

        Assert.All(operations, operation =>
            Assert.True(
                operation.Value.TryGetProperty("operationId", out var operationId)
                    && !string.IsNullOrWhiteSpace(operationId.GetString()),
                $"{operation.Key} has no operation ID."));
        var operationIds = operations.Select(operation => operation.Value.GetProperty("operationId").GetString());
        Assert.Distinct(operationIds);
    }

    // The SPA groups operations by module tag. The document tags an untagged operation with its handler's type
    // or the application name, so the rule checks that every described endpoint declares its tags itself:
    // call WithTags("<Module>") on the module's route group.
    [Fact]
    public void EveryOperationDeclaresItsTags()
    {
        Assert.Empty(OperationsWithoutDeclaredTags(factory.Services));
    }

    [Fact]
    public async Task OperationWithoutDeclaredTagsBreaksTheTagRule()
    {
        await using var app = await StartAppAsync(endpoints =>
        {
            endpoints.MapGroup("/api/v1/untagged").MapGet("/probe", () => TypedResults.NoContent()).WithName("Untagged");
            endpoints.MapGroup("/api/v1/tagged").WithTags("Competitions")
                .MapGet("/probe", () => TypedResults.NoContent()).WithName("Tagged");
        });

        Assert.Equal(["GET api/v1/untagged/probe"], OperationsWithoutDeclaredTags(app.Services));
    }

    [Fact]
    public async Task OperationThatRequiresAUserDescribesUnauthorizedAndServerErrorsAsProblemDetails()
    {
        await using var app = await StartAppAsync(endpoints =>
            endpoints.MapGet("/api/v1/probe", () => TypedResults.NoContent()).WithName("Probe").RequireAuthorization());
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe");

        Assert.Equal(ProblemDetailsRef, ProblemSchemaRef(responses, "401"));
        Assert.Equal(ProblemDetailsRef, ProblemSchemaRef(responses, "500"));
    }

    [Fact]
    public async Task OperationThatTheFallbackPolicyProtectsDescribesUnauthorizedAsProblemDetails()
    {
        await using var app = await StartAppAsync(
            endpoints => endpoints.MapGet("/api/v1/probe", () => TypedResults.NoContent()).WithName("Probe"),
            requireUserByDefault: true);
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe");

        Assert.Equal(ProblemDetailsRef, ProblemSchemaRef(responses, "401"));
    }

    [Fact]
    public async Task AnonymousOperationIsNotUnauthorizedEvenWithAFallbackPolicy()
    {
        await using var app = await StartAppAsync(
            endpoints => endpoints.MapGet("/api/v1/probe", () => TypedResults.NoContent())
                .WithName("Probe")
                .AllowAnonymous(),
            requireUserByDefault: true);
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe");

        Assert.False(responses.TryGetProperty("401", out _));
    }

    [Fact]
    public async Task OperationWithoutAuthorisationOrAFallbackPolicyIsNotUnauthorized()
    {
        await using var app = await StartAppAsync(endpoints =>
            endpoints.MapGet("/api/v1/probe", () => TypedResults.NoContent()).WithName("Probe"));
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe");

        Assert.False(responses.TryGetProperty("401", out _));
    }

    [Fact]
    public async Task AnonymousOperationDescribesServerErrorsButNotUnauthorized()
    {
        await using var app = await StartAppAsync(endpoints =>
            endpoints.MapGet("/api/v1/probe", () => TypedResults.NoContent())
                .WithName("Probe")
                .RequireAuthorization()
                .AllowAnonymous());
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe");

        Assert.False(responses.TryGetProperty("401", out _));
        Assert.Equal(ProblemDetailsRef, ProblemSchemaRef(responses, "500"));
    }

    [Fact]
    public async Task ResponsesThatTheEndpointDeclaresAreKept()
    {
        await using var app = await StartAppAsync(endpoints =>
            endpoints.MapPost("/api/v1/probe", () => TypedResults.NoContent())
                .WithName("Probe")
                .RequireAuthorization()
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status401Unauthorized));
        using var document = await GetDocumentAsync(app.GetTestClient());

        var responses = Responses(document, "/api/v1/probe", "post");

        Assert.Equal("#/components/schemas/HttpValidationProblemDetails", ProblemSchemaRef(responses, "400"));
        Assert.False(responses.GetProperty("401").TryGetProperty("content", out _));
    }

    private static async Task<WebApplication> StartAppAsync(
        Action<WebApplication> mapEndpoints,
        bool requireUserByDefault = false)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthorization(options =>
        {
            if (requireUserByDefault)
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            }
        });
        builder.Services.AddApiOpenApi();

        var app = builder.Build();
        app.MapOpenApi().AllowAnonymous();
        mapEndpoints(app);

        await app.StartAsync(CancellationToken);
        return app;
    }

    private static async Task<JsonDocument> GetDocumentAsync(HttpClient client)
    {
        using var response = await client.GetAsync(new Uri(DocumentPath, UriKind.Relative), CancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
    }

    private static IEnumerable<KeyValuePair<string, JsonElement>> Operations(JsonDocument document) =>
        document.RootElement.GetProperty("paths").EnumerateObject().SelectMany(path => path.Value.EnumerateObject()
            .Select(operation => KeyValuePair.Create($"{operation.Name.ToUpperInvariant()} {path.Name}", operation.Value)));

    private static List<string> OperationsWithoutDeclaredTags(IServiceProvider services) =>
        services.GetRequiredService<IApiDescriptionGroupCollectionProvider>().ApiDescriptionGroups.Items
            .SelectMany(group => group.Items)
            .Where(description => !description.ActionDescriptor.EndpointMetadata.OfType<ITagsMetadata>().Any())
            .Select(description => $"{description.HttpMethod} {description.RelativePath}")
            .ToList();

    private static JsonElement Responses(JsonDocument document, string path, string method = "get") =>
        document.RootElement.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses");

    private static string? ProblemSchemaRef(JsonElement responses, string statusCode) =>
        responses.GetProperty(statusCode).GetProperty("content").GetProperty("application/problem+json")
            .GetProperty("schema").GetProperty("$ref").GetString();
}
