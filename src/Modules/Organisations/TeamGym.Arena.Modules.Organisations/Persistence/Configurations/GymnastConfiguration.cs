using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Domain.Gymnasts;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

/// <summary>
/// Maps a gymnast to <c>organisations.gymnasts</c>. Only the personal data that ADR-0007 allows has a column.
/// </summary>
internal sealed class GymnastConfiguration : IEntityTypeConfiguration<Gymnast>
{
    public void Configure(EntityTypeBuilder<Gymnast> builder)
    {
        builder.ToTable("gymnasts");

        builder.HasKey(gymnast => gymnast.Id);
        builder.Property(gymnast => gymnast.Id).ValueGeneratedNever();

        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(gymnast => gymnast.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(gymnast => gymnast.GivenName).HasMaxLength(Gymnast.NameMaxLength);
        builder.Property(gymnast => gymnast.FamilyName).HasMaxLength(Gymnast.NameMaxLength);
        builder.Property(gymnast => gymnast.DateOfBirth);
        builder.Property(gymnast => gymnast.CompetitiveGender).StoreAsName();

        builder.HasXminConcurrencyToken();
    }
}
