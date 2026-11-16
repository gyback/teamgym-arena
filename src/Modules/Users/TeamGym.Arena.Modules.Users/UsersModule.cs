using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.Modules.Users;

/// <summary>
/// Registers the Users module with the API host. These are the only public types in the module (ADR-0004).
/// </summary>
public static class UsersModule
{
    /// <summary>The route prefix that every Users endpoint is mapped under.</summary>
    public const string RoutePrefix = "/api/v1/users";

    /// <summary>Registers the services of the Users module.</summary>
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }

    /// <summary>
    /// Maps the endpoints of the Users module under <see cref="RoutePrefix"/>. Feature slices map their endpoints
    /// on this group, so every module-wide convention applies to them.
    /// </summary>
    /// <returns>The route group of the module.</returns>
    public static RouteGroupBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        // Every endpoint requires an authenticated user; public endpoints opt out with AllowAnonymous().
        var group = endpoints.MapGroup(RoutePrefix)
            .WithTags("Users")
            .RequireAuthorization();

        return group;
    }
}
