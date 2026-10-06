using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class ApplicationDecisionEntityConfiguration : IEntityTypeConfiguration<ApplicationDecisionEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationDecisionEntity> builder)
    {
        builder.ToTable("ApplicationDecisions");

        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(ad => ad.ApplicationId)
            .IsRequired();

        builder.Property(ad => ad.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ad => ad.DecidedAt)
            .IsRequired();

        // Unique constraint on ApplicationId
        builder.HasIndex(ad => ad.ApplicationId)
            .IsUnique();

        // Relationship with Application
        builder.HasOne(ad => ad.Application)
            .WithOne(a => a.Decision)
            .HasForeignKey<ApplicationDecisionEntity>(ad => ad.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with RejectionReasons
        builder.HasMany(ad => ad.RejectionReasons)
            .WithOne(rr => rr.Decision)
            .HasForeignKey(rr => rr.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
