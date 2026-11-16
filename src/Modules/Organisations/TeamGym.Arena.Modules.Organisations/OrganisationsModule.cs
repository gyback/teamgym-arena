using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.Modules.Organisations;

/// <summary>
/// Registers the Organisations module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class OrganisationsModule
{
    /// <summary>The route prefix that every Organisations endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/organisations";

    /// <summary>Registers the services of the Organisations module.</summary>
    public static IServiceCollection AddOrganisationsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>
    /// Maps the endpoints of the Organisations module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapOrganisationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup(RoutePrefix).WithTags("Organisations");

        return group;
    }
}
