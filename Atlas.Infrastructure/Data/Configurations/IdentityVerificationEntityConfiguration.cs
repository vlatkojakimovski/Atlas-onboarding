using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class IdentityVerificationEntityConfiguration : IEntityTypeConfiguration<IdentityVerificationEntity>
{
    public void Configure(EntityTypeBuilder<IdentityVerificationEntity> builder)
    {
        builder.ToTable("IdentityVerifications");

        builder.HasKey(iv => iv.Id);
        builder.Property(iv => iv.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(iv => iv.ApplicationId)
            .IsRequired();

        builder.Property(iv => iv.ProviderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(iv => iv.DocumentStatus)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(iv => iv.FaceMatch)
            .IsRequired();

        builder.Property(iv => iv.Confidence)
            .IsRequired()
            .HasColumnType("DECIMAL(5,4)");

        builder.Property(iv => iv.VerifiedAt)
            .IsRequired();

        // CHECK constraint for DocumentStatus
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_IdentityVerifications_DocumentStatus",
            "[DocumentStatus] IN ('Valid', 'Invalid', 'Inconclusive')"));

        // Unique constraint on ApplicationId
        builder.HasIndex(iv => iv.ApplicationId)
            .IsUnique();

        // Relationship
        builder.HasOne(iv => iv.Application)
            .WithOne(a => a.IdentityVerification)
            .HasForeignKey<IdentityVerificationEntity>(iv => iv.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
