namespace Atlas.Infrastructure.Data.Entities;

public class DocumentEntity
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // PASSPORT, ID_CARD, SELFIE
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public DateTime UploadedAt { get; set; }

    // Navigation properties
    public ApplicationEntity Application { get; set; } = null!;
}
