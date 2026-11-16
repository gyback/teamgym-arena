using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.Modules.Applications;

/// <summary>
/// Registers the Applications module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class ApplicationsModule
{
    /// <summary>The route prefix that every Applications endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/applications";

    /// <summary>Registers the services of the Applications module.</summary>
    public static IServiceCollection AddApplicationsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>Maps the endpoints of the Applications module. Feature slices add their endpoints to this group.</summary>
    public static IEndpointRouteBuilder MapApplicationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGroup(RoutePrefix).WithTags("Applications");

        return endpoints;
    }
}
