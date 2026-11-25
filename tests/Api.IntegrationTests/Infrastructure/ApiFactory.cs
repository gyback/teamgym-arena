using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Api.IntegrationTests.Infrastructure;

/// <summary>The API host, connected to the shared <see cref="PostgresDatabase"/>.</summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "xUnit injects this fixture into public test classes' constructors, so it must be public too.")]
public sealed class ApiFactory(PostgresDatabase database) : WebApplicationFactory<Program>
{
    public PostgresDatabase Database { get; } = database;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseSetting($"ConnectionStrings:{ModuleDatabaseExtensions.ConnectionStringName}", Database.ConnectionString);
    }
}
