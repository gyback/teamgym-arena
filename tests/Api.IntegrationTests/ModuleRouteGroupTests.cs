using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using TeamGym.Arena.Modules.Applications;
using TeamGym.Arena.Modules.Competitions;
using TeamGym.Arena.Modules.Organisations;
using TeamGym.Arena.Modules.Scheduling;
using TeamGym.Arena.Modules.Users;

namespace TeamGym.Arena.Api.IntegrationTests;

public sealed class ModuleRouteGroupTests
{
    public static TheoryData<string> Modules =>
    [
        "Users",
        "Organisations",
        "Competitions",
        "Applications",
        "Scheduling",
    ];

    [Theory]
    [MemberData(nameof(Modules))]
    public async Task EndpointsOnTheModuleGroupRequireAnAuthenticatedUserAndCarryTheModuleTag(string module)
    {
        await using var app = WebApplication.CreateBuilder().Build();
        var (group, routePrefix) = MapModule(app, module);

        group.MapGet("/probe", () => TypedResults.NoContent());

        var endpoint = FindEndpoint(app, $"{routePrefix}/probe");
        Assert.Contains(module, endpoint.Metadata.GetMetadata<ITagsMetadata>()?.Tags ?? []);
        Assert.NotEmpty(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>());
        Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public async Task EndpointsOnTheModuleGroupCanOptOutOfAuthentication(string module)
    {
        await using var app = WebApplication.CreateBuilder().Build();
        var (group, routePrefix) = MapModule(app, module);

        group.MapGet("/public", () => TypedResults.NoContent()).AllowAnonymous();

        var endpoint = FindEndpoint(app, $"{routePrefix}/public");
        Assert.NotNull(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
    }

    private static (RouteGroupBuilder Group, string RoutePrefix) MapModule(WebApplication app, string module) => module switch
    {
        "Users" => (app.MapUsersEndpoints(), UsersModule.RoutePrefix),
        "Organisations" => (app.MapOrganisationsEndpoints(), OrganisationsModule.RoutePrefix),
        "Competitions" => (app.MapCompetitionsEndpoints(), CompetitionsModule.RoutePrefix),
        "Applications" => (app.MapApplicationsEndpoints(), ApplicationsModule.RoutePrefix),
        "Scheduling" => (app.MapSchedulingEndpoints(), SchedulingModule.RoutePrefix),
        _ => throw new ArgumentOutOfRangeException(nameof(module), module, "Unknown module."),
    };

    private static RouteEndpoint FindEndpoint(IEndpointRouteBuilder endpoints, string routePattern) => Assert.Single(
        endpoints.DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>(),
        endpoint => endpoint.RoutePattern.RawText == routePattern);
}
