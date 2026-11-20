using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace TeamGym.Arena.Api.Spa;

/// <summary>
/// Serves the compiled React SPA from <c>wwwroot</c> on the same origin as the API and the BFF (ADR-0005).
/// The container build copies the Vite output (<c>src/Web/dist</c>) into <c>wwwroot</c>. Locally the Vite dev
/// server serves the SPA instead and proxies the backend paths to this host, so <c>wwwroot</c> is usually empty.
/// </summary>
internal static class SpaHostingExtensions
{
    private const string IndexFile = "index.html";

    // Client-side routes: any path with no file extension that is not under a backend path the SPA calls on
    // the same origin. An unknown /api/... route must stay a 404 problem details response instead of returning
    // the SPA shell with 200. The root path is served by UseDefaultFiles, because a catch-all parameter has no
    // value there for the constraint to match.
    private const string ClientRoutePattern = "{*path:regex(^(?!(api|bff|hubs)(/|$))):nonfile}";

    // Vite emits content-hashed bundles under /assets, so a changed file always gets a new URL.
    private static readonly PathString s_hashedAssetsPath = new("/assets");

    private static readonly CacheControlHeaderValue s_immutable = new()
    {
        Public = true,
        MaxAge = TimeSpan.FromDays(365),
        Extensions = { new NameValueHeaderValue("immutable") },
    };

    // index.html and unhashed files must be revalidated, so a deployment reaches every browser at once.
    private static readonly CacheControlHeaderValue s_revalidate = new() { NoCache = true };

    /// <summary>Serves the SPA's static files, and <c>index.html</c> for <c>/</c>. Call it before authorisation.</summary>
    public static WebApplication UseSpaStaticFiles(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = SetCacheHeaders });
        return app;
    }

    /// <summary>
    /// Answers every client-side route with <c>index.html</c>, so deep links and reloads work. Map it after every
    /// other endpoint.
    /// </summary>
    public static WebApplication MapSpaFallback(this WebApplication app)
    {
        app.MapFallbackToFile(ClientRoutePattern, IndexFile, new StaticFileOptions { OnPrepareResponse = SetCacheHeaders })
            // The SPA shell is public: it must load before the user can sign in. It holds no data, and every API
            // call it makes is authorised on its own (ADR-0008).
            .AllowAnonymous();
        return app;
    }

    private static void SetCacheHeaders(StaticFileResponseContext context)
    {
        var isHashedAsset = context.Context.Request.Path.StartsWithSegments(s_hashedAssetsPath, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(context.File.Name, IndexFile, StringComparison.OrdinalIgnoreCase);

        context.Context.Response.GetTypedHeaders().CacheControl = isHashedAsset ? s_immutable : s_revalidate;
    }
}
