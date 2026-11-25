using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamGym.Arena.Api.IntegrationTests.Infrastructure;
using TeamGym.Arena.Common.Events;
using TeamGym.Arena.Common.Persistence;

namespace TeamGym.Arena.Api.IntegrationTests.Persistence;

/// <summary>
/// The shared domain event dispatch of ADR-0007, through a module-like <c>DbContext</c> that lives only in this test,
/// in its own schema.
/// </summary>
public sealed class DomainEventDispatchTests(PostgresDatabase database) : IAsyncLifetime
{
    internal const string Schema = "domain_event_tests";

    private IHost? _host;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("The host has not started.");

    public async ValueTask InitializeAsync()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Configuration[$"ConnectionStrings:{ModuleDatabaseExtensions.ConnectionStringName}"] = database.ConnectionString;
        builder.AddModuleDbContext<WidgetDbContext>(Schema);
        builder.Services.AddScoped<IDomainEventConsumer<WidgetRenamed>>(
            services => new RenameLogger(services.GetRequiredService<WidgetDbContext>()));
        _host = builder.Build();

        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();
        await DropSchemaAsync(context);
        await context.GetService<IRelationalDatabaseCreator>().CreateTablesAsync(CancellationToken);
    }

    // The database is shared with the module tests, which check that every table is in a module schema.
    public async ValueTask DisposeAsync()
    {
        if (_host is null)
        {
            return;
        }

        await using (var scope = _host.Services.CreateAsyncScope())
        {
            await DropSchemaAsync(scope.ServiceProvider.GetRequiredService<WidgetDbContext>());
        }

        _host.Dispose();
    }

    [Fact]
    public async Task ConsumersRunBeforeSavingAndTheirChangesAreSavedWithTheAggregate()
    {
        var widget = new Widget(Guid.CreateVersion7());
        widget.Rename("First", followUps: 0);

        await using (var scope = Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();
            context.Widgets.Add(widget);
            await context.SaveChangesAsync(CancellationToken);
        }

        Assert.Empty(widget.DomainEvents);
        Assert.Equal(["First"], await LoggedNamesAsync());
    }

    [Fact]
    public async Task EventsThatConsumersRaiseAreDispatchedInTheSameSave()
    {
        var widget = new Widget(Guid.CreateVersion7());
        widget.Rename("Name", followUps: 2);

        await using (var scope = Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();
            context.Widgets.Add(widget);
            await context.SaveChangesAsync(CancellationToken);
        }

        Assert.Equal(["Name", "Name!", "Name!!"], await LoggedNamesAsync());
    }

    [Fact]
    public async Task ConsumersThatKeepRaisingEventsFailTheSaveInsteadOfLoopingForever()
    {
        var widget = new Widget(Guid.CreateVersion7());
        widget.Rename("Name", followUps: DomainEventDispatchInterceptor.MaxDispatchRounds);

        await using (var scope = Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();
            context.Widgets.Add(widget);
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync(CancellationToken));
        }

        Assert.Empty(await LoggedNamesAsync());
    }

    [Fact]
    public async Task SynchronousSaveChangesRefusesToDropPendingEvents()
    {
        var widget = new Widget(Guid.CreateVersion7());
        widget.Rename("Name", followUps: 0);

        await using (var scope = Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();
            context.Widgets.Add(widget);
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        }

        Assert.Empty(await LoggedNamesAsync());
    }

    private static Task<int> DropSchemaAsync(WidgetDbContext context) =>
        context.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS {Schema} CASCADE", CancellationToken.None);

    private async Task<List<string>> LoggedNamesAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<WidgetDbContext>();

        return await context.RenameLog.OrderBy(entry => entry.Sequence).Select(entry => entry.Name).ToListAsync(CancellationToken);
    }
}

[SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "AddModuleDbContext registers it, and dependency injection creates it.")]
internal sealed class WidgetDbContext(DbContextOptions<WidgetDbContext> options) : ModuleDbContext(options, DomainEventDispatchTests.Schema)
{
    public DbSet<Widget> Widgets => Set<Widget>();

    public DbSet<RenameLogEntry> RenameLog => Set<RenameLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Widget>(widget =>
        {
            widget.HasKey(entity => entity.Id);
            widget.Property(entity => entity.Id).ValueGeneratedNever();
            widget.Ignore(entity => entity.DomainEvents);
        });

        modelBuilder.Entity<RenameLogEntry>().HasKey(entry => entry.Sequence);
    }
}

internal sealed class Widget(Guid id) : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; } = id;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public void Rename(string name, int followUps)
    {
        Name = name;
        _domainEvents.Add(new WidgetRenamed(Id, name, followUps, DateTimeOffset.UnixEpoch));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <param name="FollowUps">How many more renames the consumer triggers, each raising another event.</param>
internal sealed record WidgetRenamed(Guid WidgetId, string Name, int FollowUps, DateTimeOffset OccurredAt) : IDomainEvent;

internal sealed class RenameLogEntry(string name)
{
    public int Sequence { get; private set; }

    public string Name { get; private set; } = name;
}

internal sealed class RenameLogger(WidgetDbContext context) : IDomainEventConsumer<WidgetRenamed>
{
    public Task HandleAsync(WidgetRenamed domainEvent, CancellationToken cancellationToken)
    {
        context.RenameLog.Add(new RenameLogEntry(domainEvent.Name));

        if (domainEvent.FollowUps > 0)
        {
            var widget = context.Widgets.Local.Single(candidate => candidate.Id == domainEvent.WidgetId);
            widget.Rename(domainEvent.Name + "!", domainEvent.FollowUps - 1);
        }

        return Task.CompletedTask;
    }
}
