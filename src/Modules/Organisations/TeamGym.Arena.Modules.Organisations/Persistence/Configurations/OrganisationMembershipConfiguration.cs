using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

internal sealed class OrganisationMembershipConfiguration : IEntityTypeConfiguration<OrganisationMembership>
{
    public void Configure(EntityTypeBuilder<OrganisationMembership> builder)
    {
        builder.ToTable("organisation_memberships");

        builder.HasKey(membership => membership.Id);
        builder.Property(membership => membership.Id).ValueGeneratedNever();

        // Users belong to the Users module, so this is a plain uuid without a foreign key (ADR-0007). The index
        // serves the permission checks, which look up a user's memberships (ADR-0008).
        builder.Property(membership => membership.UserId);
        builder.HasIndex(membership => membership.UserId);

        builder.Property(membership => membership.Role).StoreAsName();
        builder.Property(membership => membership.JoinedAt);

        // The aggregate rejects a duplicate role; the constraint also stops two concurrent requests.
        builder.HasIndex(membership => new { membership.OrganisationId, membership.UserId, membership.Role })
            .IsUnique();

        builder.HasMany<CoachTeamAssignment>("_teamAssignments")
            .WithOne()
            .HasForeignKey(assignment => assignment.OrganisationMembershipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_teamAssignments").AutoInclude();

        builder.Ignore(membership => membership.TeamIds);
    }
}
