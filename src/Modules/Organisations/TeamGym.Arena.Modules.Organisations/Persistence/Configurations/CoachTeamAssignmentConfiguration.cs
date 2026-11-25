using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

internal sealed class CoachTeamAssignmentConfiguration : IEntityTypeConfiguration<CoachTeamAssignment>
{
    public void Configure(EntityTypeBuilder<CoachTeamAssignment> builder)
    {
        builder.ToTable("coach_team_assignments");

        builder.HasKey(assignment => new { assignment.OrganisationMembershipId, assignment.TeamId });

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(assignment => assignment.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
