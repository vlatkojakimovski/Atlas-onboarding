using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class CardOrderEntityConfiguration : IEntityTypeConfiguration<CardOrderEntity>
{
    public void Configure(EntityTypeBuilder<CardOrderEntity> builder)
    {
        builder.ToTable("CardOrders");

        builder.HasKey(co => co.Id);
        builder.Property(co => co.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(co => co.ApplicationId)
            .IsRequired();

        builder.Property(co => co.AccountId)
            .IsRequired();

        builder.Property(co => co.CardOrderId)
            .IsRequired();

        builder.Property(co => co.CardReference)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(co => co.RequiresBranchActivation)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(co => co.OrderedAt)
            .IsRequired();

        // Unique constraint on ApplicationId
        builder.HasIndex(co => co.ApplicationId)
            .IsUnique();

        // Relationship
        builder.HasOne(co => co.Application)
            .WithOne(a => a.CardOrder)
            .HasForeignKey<CardOrderEntity>(co => co.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
