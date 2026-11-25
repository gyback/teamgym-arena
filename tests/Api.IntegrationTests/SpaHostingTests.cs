using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;

namespace TeamGym.Arena.Api.IntegrationTests;

/// <summary>
/// The API serves the built SPA from its web root in production (ADR-0005). These tests use a temporary web
/// root that looks like the Vite output.
/// </summary>
public sealed class SpaHostingTests : IClassFixture<ApiFactory>, IDisposable
{
    private const string IndexHtml = "<!doctype html><title>TeamGym Arena</title><div id=\"root\"></div>";
    private const string HashedAsset = "/assets/index-AbC123.js";

    private readonly string _webRoot = Directory.CreateTempSubdirectory("teamgym-arena-wwwroot-").FullName;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpaHostingTests(ApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        File.WriteAllText(Path.Combine(_webRoot, "index.html"), IndexHtml);
        Directory.CreateDirectory(Path.Combine(_webRoot, "assets"));
        File.WriteAllText(Path.Combine(_webRoot, "assets", "index-AbC123.js"), "export {};");

        _factory = factory.WithWebHostBuilder(host => host.UseEnvironment("Production").UseWebRoot(_webRoot));
        _client = _factory.CreateClient();
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        Directory.Delete(_webRoot, recursive: true);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/competitions")]
    [InlineData("/competitions/0193b0f4-6c2f-7a3e-9d5c-1f2e3d4c5b6a/schedule")]
    [InlineData("/apiary")]
    public async Task ClientSideRouteReturnsTheSpaShellThatBrowsersMustRevalidate(string path)
    {
        using var response = await _client.GetAsync(new Uri(path, UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(IndexHtml, await response.Content.ReadAsStringAsync(CancellationToken));
        Assert.True(response.Headers.CacheControl?.NoCache);
    }

    [Fact]
    public async Task HashedAssetIsCachedAsImmutable()
    {
        using var response = await _client.GetAsync(new Uri(HashedAsset, UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TimeSpan.FromDays(365), response.Headers.CacheControl?.MaxAge);
        Assert.Contains(response.Headers.CacheControl?.Extensions ?? [], extension => extension.Name == "immutable");
    }

    [Theory]
    [InlineData("/api")]
    [InlineData("/api/v1/does-not-exist")]
    [InlineData("/API/v1/does-not-exist")]
    [InlineData("/bff/does-not-exist")]
    [InlineData("/hubs/does-not-exist")]
    [InlineData("/assets/missing-XyZ789.js")]
    public async Task BackendPathOrMissingFileReturnsProblemDetailsInsteadOfTheSpaShell(string path)
    {
        using var response = await _client.GetAsync(new Uri(path, UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HealthEndpointTakesPrecedenceOverTheSpaShell()
    {
        using var response = await _client.GetAsync(new Uri("/health", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken));
    }
}
