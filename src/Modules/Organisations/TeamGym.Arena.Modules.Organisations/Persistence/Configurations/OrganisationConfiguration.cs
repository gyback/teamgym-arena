using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamGym.Arena.Common.Persistence;
using TeamGym.Arena.Modules.Organisations.Domain.Organisations;

namespace TeamGym.Arena.Modules.Organisations.Persistence.Configurations;

internal sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");

        builder.HasKey(organisation => organisation.Id);
        builder.Property(organisation => organisation.Id).ValueGeneratedNever();

        builder.Property(organisation => organisation.Name).HasMaxLength(Organisation.NameMaxLength);
        builder.Property(organisation => organisation.ShortName).HasMaxLength(Organisation.ShortNameMaxLength);
        builder.Property(organisation => organisation.Type).StoreAsName();
        builder.Property(organisation => organisation.Status).StoreAsName();

        builder.Property(organisation => organisation.CountryCode)
            .HasConversion(code => code.Value, value => CountryCode.Create(value).Value)
            .HasMaxLength(2)
            .IsFixedLength();

        // The hierarchy stays inside this schema, so it is a real foreign key. An organisation with member
        // organisations cannot be deleted.
        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(organisation => organisation.ParentOrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Rule sets belong to the Competitions module, so this is a plain uuid without a foreign key (ADR-0007).
        builder.Property(organisation => organisation.DefaultRuleSetId);

        builder.Ignore(organisation => organisation.IsActive);

        builder.HasMany(organisation => organisation.Memberships)
            .WithOne()
            .HasForeignKey(membership => membership.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);

        // The aggregate always loads whole, so that its invariants see every membership.
        builder.Navigation(organisation => organisation.Memberships).AutoInclude();

        builder.HasXminConcurrencyToken();
    }
}
