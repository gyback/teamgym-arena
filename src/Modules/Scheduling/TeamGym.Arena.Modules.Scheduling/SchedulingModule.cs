using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.Modules.Scheduling;

/// <summary>
/// Registers the Scheduling module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class SchedulingModule
{
    /// <summary>The route prefix that every Scheduling endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/schedules";

    /// <summary>Registers the services of the Scheduling module.</summary>
    public static IServiceCollection AddSchedulingModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>Maps the endpoints of the Scheduling module. Feature slices add their endpoints to this group.</summary>
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGroup(RoutePrefix).WithTags("Scheduling");

        return endpoints;
    }
}
