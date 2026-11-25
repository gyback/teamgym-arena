using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace TeamGym.Arena.MigrationService.DesignTime;

/// <summary>
/// Customises <c>dotnet ef</c>, which uses this project as its startup project. Only the tools use it; the migration
/// runner itself never does.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "dotnet ef finds and creates the design-time services of the startup project by reflection.")]
internal sealed class MigrationsDesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection) =>
        serviceCollection.AddSingleton<IMigrationsCodeGenerator>(services => new InternalMigrationsGenerator(
            services.GetRequiredService<MigrationsCodeGeneratorDependencies>(),
            services.GetRequiredService<CSharpMigrationsGeneratorDependencies>()));
}
