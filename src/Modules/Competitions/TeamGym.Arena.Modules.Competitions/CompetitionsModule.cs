using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.Modules.Competitions;

/// <summary>
/// Registers the Competitions module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class CompetitionsModule
{
    /// <summary>The route prefix that every Competitions endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/competitions";

    /// <summary>Registers the services of the Competitions module.</summary>
    public static IServiceCollection AddCompetitionsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>
    /// Maps the endpoints of the Competitions module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapCompetitionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup(RoutePrefix).WithTags("Competitions");

        return group;
    }
}
