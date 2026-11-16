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

    /// <summary>Maps the endpoints of the Competitions module. Feature slices add their endpoints to this group.</summary>
    public static IEndpointRouteBuilder MapCompetitionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGroup(RoutePrefix).WithTags("Competitions");

        return endpoints;
    }
}
