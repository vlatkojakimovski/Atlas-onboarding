using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class RejectionReasonEntityConfiguration : IEntityTypeConfiguration<RejectionReasonEntity>
{
    public void Configure(EntityTypeBuilder<RejectionReasonEntity> builder)
    {
        builder.ToTable("RejectionReasons");

        builder.HasKey(rr => rr.Id);
        builder.Property(rr => rr.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(rr => rr.DecisionId)
            .IsRequired();

        builder.Property(rr => rr.Reason)
            .IsRequired()
            .HasMaxLength(50);

        // Relationship
        builder.HasOne(rr => rr.Decision)
            .WithMany(d => d.RejectionReasons)
            .HasForeignKey(rr => rr.DecisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
