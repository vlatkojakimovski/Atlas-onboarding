using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class AccountDetailsEntityConfiguration : IEntityTypeConfiguration<AccountDetailsEntity>
{
    public void Configure(EntityTypeBuilder<AccountDetailsEntity> builder)
    {
        builder.ToTable("AccountDetails");

        builder.HasKey(ad => ad.Id);
        builder.Property(ad => ad.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(ad => ad.ApplicationId)
            .IsRequired();

        builder.Property(ad => ad.AccountId)
            .IsRequired();

        builder.Property(ad => ad.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ad => ad.CreatedAt)
            .IsRequired();

        // Unique constraint on ApplicationId
        builder.HasIndex(ad => ad.ApplicationId)
            .IsUnique();

        // Relationship
        builder.HasOne(ad => ad.Application)
            .WithOne(a => a.AccountDetails)
            .HasForeignKey<AccountDetailsEntity>(ad => ad.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
