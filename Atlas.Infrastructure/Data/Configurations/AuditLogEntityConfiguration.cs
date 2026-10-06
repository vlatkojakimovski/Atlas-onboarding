using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);
        builder.Property(al => al.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(al => al.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(al => al.UserId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(al => al.ApplicationId)
            .IsRequired(false);

        builder.Property(al => al.MarketCode)
            .HasMaxLength(2)
            .IsRequired(false);

        builder.Property(al => al.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(al => al.ResourceType)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(al => al.ResourceId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(al => al.Action)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(al => al.Details)
            .HasColumnType("NVARCHAR(MAX)")
            .IsRequired(false);

        // Indexes for compliance queries
        builder.HasIndex(al => al.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(al => al.ApplicationId)
            .HasDatabaseName("IX_AuditLogs_ApplicationId");

        builder.HasIndex(al => al.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(al => al.MarketCode)
            .HasDatabaseName("IX_AuditLogs_MarketCode");

        // Relationship
        builder.HasOne(al => al.Application)
            .WithMany(a => a.AuditLogs)
            .HasForeignKey(al => al.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);  // ApplicationId is nullable
    }
}
