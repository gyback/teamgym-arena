using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Api.IntegrationTests;

public sealed class HostTests(ApiFactory factory)
    : IClassFixture<ApiFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose() => _client.Dispose();

    [Theory]
    [InlineData("/alive")]
    [InlineData("/health")]
    public async Task HealthEndpointReportsHealthy(string path)
    {
        using var response = await _client.GetAsync(new Uri(path, UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken));
    }

    [Theory]
    [InlineData("/alive")]
    [InlineData("/health")]
    public async Task HealthEndpointIsMappedOutsideDevelopment(string path)
    {
        using var productionFactory = factory.WithWebHostBuilder(host => host.UseEnvironment("Production"));
        using var client = productionFactory.CreateClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task ReadinessReportsUnhealthyWhileTheDatabaseIsUnreachableButLivenessDoesNot()
    {
        // Port 1 on the loopback interface refuses connections, so every module's database check fails fast.
        using var offlineFactory = factory.WithWebHostBuilder(host => host.UseSetting(
            $"ConnectionStrings:{ModuleDatabaseExtensions.ConnectionStringName}", "Host=127.0.0.1;Port=1;Database=teamgym_arena;Username=teamgym;Timeout=2"));
        using var client = offlineFactory.CreateClient();

        var stopwatch = Stopwatch.StartNew();
        using var health = await client.GetAsync(new Uri("/health", UriKind.Relative), CancellationToken);
        stopwatch.Stop();
        using var alive = await client.GetAsync(new Uri("/alive", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, health.StatusCode);
        // The retrying execution strategy must not keep a platform probe waiting.
        Assert.InRange(stopwatch.Elapsed, TimeSpan.Zero, ModuleDatabaseExtensions.HealthCheckTimeout * 3);
        Assert.Equal("Unhealthy", await health.Content.ReadAsStringAsync(CancellationToken));
        Assert.Equal(HttpStatusCode.OK, alive.StatusCode);
    }

    [Fact]
    public async Task OpenApiDocumentUsesOpenApi31()
    {
        using var response = await _client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
        Assert.StartsWith("3.1", document.RootElement.GetProperty("openapi").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownRouteReturnsProblemDetails()
    {
        using var response = await _client.GetAsync(new Uri("/api/v1/does-not-exist", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken);
        Assert.Equal((int)HttpStatusCode.NotFound, problem?.Status);
    }
}
