using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");

        builder.HasKey(team => team.Id);
        builder.Property(team => team.Id).ValueGeneratedNever();

        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(team => team.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(team => team.Name).HasMaxLength(Team.NameMaxLength);
        builder.Property(team => team.Section).StoreAsName();

        builder.HasMany<TeamMember>("_members")
            .WithOne()
            .HasForeignKey(member => member.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate always loads whole, so that its invariants see every member.
        builder.Navigation("_members").AutoInclude();

        builder.Ignore(team => team.GymnastIds);

        builder.HasXminConcurrencyToken();
    }
}
