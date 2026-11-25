using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Persistence;

namespace TeamGym.Arena.Modules.Organisations;

/// <summary>
/// Registers the Organisations module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class OrganisationsModule
{
    /// <summary>The route prefix that every Organisations endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/organisations";

    /// <summary>Registers the services of the Organisations module, including its database.</summary>
    public static IHostApplicationBuilder AddOrganisationsModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddOrganisationsPersistence();
    }

    /// <summary>
    /// Registers only the database of the Organisations module, in the <c>organisations</c> schema. The migration runner uses it
    /// to migrate the schema without starting the module's other services (ADR-0007).
    /// </summary>
    public static IHostApplicationBuilder AddOrganisationsPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddModuleDbContext<OrganisationsDbContext>(OrganisationsDbContext.Schema);
    }

    /// <summary>
    /// Maps the endpoints of the Organisations module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapOrganisationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Every endpoint requires an authenticated user; public endpoints opt out with AllowAnonymous().
        var group = endpoints.MapGroup(RoutePrefix)
            .WithTags("Organisations")
            .RequireAuthorization();

        return group;
    }
}
