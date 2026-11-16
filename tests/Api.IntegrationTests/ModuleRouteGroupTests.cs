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
    public static TheoryData<string, string> Modules => new()
    {
        { nameof(UsersModule), "Users" },
        { nameof(OrganisationsModule), "Organisations" },
        { nameof(CompetitionsModule), "Competitions" },
        { nameof(ApplicationsModule), "Applications" },
        { nameof(SchedulingModule), "Scheduling" },
    };

    [Theory]
    [MemberData(nameof(Modules))]
    public async Task EndpointsMappedOnTheModuleGroupInheritItsConventions(string module, string expectedTag)
    {
        await using var app = WebApplication.CreateBuilder().Build();
        var (group, routePrefix) = MapModule(app, module);

        group.MapGet("/probe", () => TypedResults.NoContent());

        var endpoint = Assert.Single(
            ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>(),
            candidate => candidate.RoutePattern.RawText == $"{routePrefix}/probe");
        Assert.Contains(expectedTag, endpoint.Metadata.GetMetadata<ITagsMetadata>()?.Tags ?? []);
    }

    private static (RouteGroupBuilder Group, string RoutePrefix) MapModule(WebApplication app, string module) => module switch
    {
        nameof(UsersModule) => (app.MapUsersEndpoints(), UsersModule.RoutePrefix),
        nameof(OrganisationsModule) => (app.MapOrganisationsEndpoints(), OrganisationsModule.RoutePrefix),
        nameof(CompetitionsModule) => (app.MapCompetitionsEndpoints(), CompetitionsModule.RoutePrefix),
        nameof(ApplicationsModule) => (app.MapApplicationsEndpoints(), ApplicationsModule.RoutePrefix),
        nameof(SchedulingModule) => (app.MapSchedulingEndpoints(), SchedulingModule.RoutePrefix),
        _ => throw new ArgumentOutOfRangeException(nameof(module), module, "Unknown module."),
    };
}
