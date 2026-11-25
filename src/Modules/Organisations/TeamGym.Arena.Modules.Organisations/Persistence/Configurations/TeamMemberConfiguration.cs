using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Teams;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

internal sealed class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");

        builder.HasKey(member => new { member.TeamId, member.GymnastId });

        // A deleted gymnast leaves every team (ADR-0007 erasure, #28).
        builder.HasOne<Gymnast>()
            .WithMany()
            .HasForeignKey(member => member.GymnastId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
