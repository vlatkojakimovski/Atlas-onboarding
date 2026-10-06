using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class ApplicationEntityConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.DateOfBirth)
            .IsRequired();

        builder.Property(a => a.MarketCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(a => a.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(a => a.CompletedAt)
            .IsRequired(false);

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // CHECK constraints for Status
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Applications_Status",
            "[Status] IN ('Pending', 'Approved', 'Rejected', 'PendingReview')"));

        // CHECK constraints for MarketCode
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Applications_MarketCode",
            "[MarketCode] IN ('MA', 'MB', 'MC', 'MD', 'ME', 'MF')"));

        // Indexes
        builder.HasIndex(a => new { a.MarketCode, a.Status })
            .HasDatabaseName("IX_Applications_MarketCode_Status");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_Applications_CreatedAt");

        builder.HasIndex(a => a.Email)
            .HasDatabaseName("IX_Applications_Email");

        // Relationships - configured in dependent entity configurations to avoid duplicate mappings
    }
}
