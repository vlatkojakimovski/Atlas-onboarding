using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class SanctionsScreeningEntityConfiguration : IEntityTypeConfiguration<SanctionsScreeningEntity>
{
    public void Configure(EntityTypeBuilder<SanctionsScreeningEntity> builder)
    {
        builder.ToTable("SanctionsScreenings");

        builder.HasKey(ss => ss.Id);
        builder.Property(ss => ss.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(ss => ss.ApplicationId)
            .IsRequired();

        builder.Property(ss => ss.CaseId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ss => ss.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ss => ss.ScreenedAt)
            .IsRequired();

        // CHECK constraint for Status
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SanctionsScreenings_Status",
            "[Status] IN ('Clear', 'PossibleMatch')"));

        // Unique constraint on ApplicationId
        builder.HasIndex(ss => ss.ApplicationId)
            .IsUnique();

        // Relationship with Application
        builder.HasOne(ss => ss.Application)
            .WithOne(a => a.SanctionsScreening)
            .HasForeignKey<SanctionsScreeningEntity>(ss => ss.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with SanctionMatches
        builder.HasMany(ss => ss.Matches)
            .WithOne(sm => sm.Screening)
            .HasForeignKey(sm => sm.ScreeningId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
