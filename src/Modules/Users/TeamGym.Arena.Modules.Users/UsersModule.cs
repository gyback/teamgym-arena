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

    /// <summary>Maps the endpoints of the Users module. Feature slices add their endpoints to this group.</summary>
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGroup(RoutePrefix).WithTags("Users");

        return endpoints;
    }
}
