using Microsoft.EntityFrameworkCore;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;
using TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

namespace TeamGym.Arena.Modules.Organisations.Persistence;

/// <summary>
/// The database context of the Organisations module. Its tables live in the <c>organisations</c> schema (ADR-0007).
/// </summary>
internal sealed class OrganisationsDbContext(DbContextOptions<OrganisationsDbContext> options) : ModuleDbContext(options, Schema)
{
    public const string Schema = "organisations";

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<Gymnast> Gymnasts => Set<Gymnast>();

    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .ApplyConfiguration(new OrganisationConfiguration())
            .ApplyConfiguration(new OrganisationMembershipConfiguration())
            .ApplyConfiguration(new CoachTeamAssignmentConfiguration())
            .ApplyConfiguration(new GymnastConfiguration())
            .ApplyConfiguration(new TeamConfiguration())
            .ApplyConfiguration(new TeamMemberConfiguration());
    }
}
