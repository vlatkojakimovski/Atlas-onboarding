using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class SanctionMatchEntityConfiguration : IEntityTypeConfiguration<SanctionMatchEntity>
{
    public void Configure(EntityTypeBuilder<SanctionMatchEntity> builder)
    {
        builder.ToTable("SanctionMatches");

        builder.HasKey(sm => sm.Id);
        builder.Property(sm => sm.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(sm => sm.ScreeningId)
            .IsRequired();

        builder.Property(sm => sm.MatchType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sm => sm.Score)
            .IsRequired()
            .HasColumnType("DECIMAL(5,4)");

        builder.Property(sm => sm.Subject)
            .IsRequired()
            .HasMaxLength(500);

        // CHECK constraint for MatchType
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SanctionMatches_Type",
            "[MatchType] IN ('Sanctions', 'PEP')"));

        // Relationship
        builder.HasOne(sm => sm.Screening)
            .WithMany(s => s.Matches)
            .HasForeignKey(sm => sm.ScreeningId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
