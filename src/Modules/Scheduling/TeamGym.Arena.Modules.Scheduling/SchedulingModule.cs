using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Scheduling.Persistence;

namespace TeamGym.Arena.Modules.Scheduling;

/// <summary>
/// Registers the Scheduling module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class SchedulingModule
{
    /// <summary>The route prefix that every Scheduling endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/schedules";

    /// <summary>Registers the services of the Scheduling module, including its database.</summary>
    public static IHostApplicationBuilder AddSchedulingModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddSchedulingPersistence();
    }

    /// <summary>
    /// Registers only the database of the Scheduling module, in the <c>scheduling</c> schema. The migration runner uses it
    /// to migrate the schema without starting the module's other services (ADR-0007).
    /// </summary>
    public static IHostApplicationBuilder AddSchedulingPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddModuleDbContext<SchedulingDbContext>(SchedulingDbContext.Schema);
    }

    /// <summary>
    /// Maps the endpoints of the Scheduling module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Every endpoint requires an authenticated user; public endpoints opt out with AllowAnonymous().
        var group = endpoints.MapGroup(RoutePrefix)
            .WithTags("Scheduling")
            .RequireAuthorization();

        return group;
    }
}
