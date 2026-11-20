using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

    // Generated client methods and TanStack Query keys are named after operation IDs, and the SPA groups
    // operations by module tag. Name every endpoint with WithName(), and map it on its module's route group.
    [Fact]
    public async Task EveryOperationHasAUniqueOperationIdAndATag()
    {
        using var client = factory.CreateClient();
        using var document = await GetDocumentAsync(client);

        var operations = Operations(document).ToList();

        Assert.All(operations, operation =>
        {
            Assert.True(
                operation.Value.TryGetProperty("operationId", out var operationId)
                    && !string.IsNullOrWhiteSpace(operationId.GetString()),
                $"{operation.Key} has no operation ID.");
            Assert.True(
                operation.Value.TryGetProperty("tags", out var tags) && tags.GetArrayLength() > 0,
                $"{operation.Key} has no tag.");
        });
        var operationIds = operations.Select(operation => operation.Value.GetProperty("operationId").GetString());
        Assert.Distinct(operationIds);
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

    private static async Task<WebApplication> StartAppAsync(Action<WebApplication> mapEndpoints)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthorization();
        builder.Services.AddApiOpenApi();

        var app = builder.Build();
        app.MapOpenApi();
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

    private static JsonElement Responses(JsonDocument document, string path, string method = "get") =>
        document.RootElement.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses");

    private static string? ProblemSchemaRef(JsonElement responses, string statusCode) =>
        responses.GetProperty(statusCode).GetProperty("content").GetProperty("application/problem+json")
            .GetProperty("schema").GetProperty("$ref").GetString();
}
