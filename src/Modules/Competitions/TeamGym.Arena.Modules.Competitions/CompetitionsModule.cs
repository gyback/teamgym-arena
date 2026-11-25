using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Competitions.Persistence;

namespace TeamGym.Arena.Modules.Competitions;

/// <summary>
/// Registers the Competitions module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class CompetitionsModule
{
    /// <summary>The route prefix that every Competitions endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/competitions";

    /// <summary>Registers the services of the Competitions module, including its database.</summary>
    public static IHostApplicationBuilder AddCompetitionsModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddCompetitionsPersistence();
    }

    /// <summary>
    /// Registers only the database of the Competitions module, in the <c>competitions</c> schema. The migration runner uses it
    /// to migrate the schema without starting the module's other services (ADR-0007).
    /// </summary>
    public static IHostApplicationBuilder AddCompetitionsPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddModuleDbContext<CompetitionsDbContext>(CompetitionsDbContext.Schema);
    }

    /// <summary>
    /// Maps the endpoints of the Competitions module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapCompetitionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Every endpoint requires an authenticated user; public endpoints opt out with AllowAnonymous().
        var group = endpoints.MapGroup(RoutePrefix)
            .WithTags("Competitions")
            .RequireAuthorization();

        return group;
    }
}
