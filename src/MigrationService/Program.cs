using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Applications;
using TeamGym.Arena.Modules.Competitions;
using TeamGym.Arena.Modules.Organisations;
using TeamGym.Arena.Modules.Scheduling;
using TeamGym.Arena.Modules.Users;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Only the modules' databases, not their other services, so that nothing but the migrations runs here.
builder
    .AddUsersPersistence()
    .AddOrganisationsPersistence()
    .AddCompetitionsPersistence()
    .AddApplicationsPersistence()
    .AddSchedulingPersistence();

using var host = builder.Build();

await host.StartAsync().ConfigureAwait(false);

try
{
    var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TeamGym.Arena.MigrationService");
    var stopping = host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;

    // A failed migration throws, so the process exits with a non-zero code and the deployment stops (ADR-0009).
    await ModuleDatabaseMigrator.MigrateAsync(host.Services, logger, stopping).ConfigureAwait(false);
}
finally
{
    // Stopping the host flushes the telemetry exporters before the process exits.
    await host.StopAsync().ConfigureAwait(false);
}
