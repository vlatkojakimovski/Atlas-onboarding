using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Atlas.Infrastructure.Data.Entities;

namespace Atlas.Infrastructure.Data.Configurations;

public class DocumentEntityConfiguration : IEntityTypeConfiguration<DocumentEntity>
{
    public void Configure(EntityTypeBuilder<DocumentEntity> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(d => d.ApplicationId)
            .IsRequired();

        builder.Property(d => d.DocumentType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.ImageData)
            .IsRequired()
            .HasColumnType("VARBINARY(MAX)");

        builder.Property(d => d.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // CHECK constraint for DocumentType
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Documents_Type",
            "[DocumentType] IN ('PASSPORT', 'ID_CARD', 'SELFIE')"));

        // Index
        builder.HasIndex(d => d.ApplicationId)
            .HasDatabaseName("IX_Documents_ApplicationId");

        // Relationship
        builder.HasOne(d => d.Application)
            .WithMany(a => a.Documents)
            .HasForeignKey(d => d.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
