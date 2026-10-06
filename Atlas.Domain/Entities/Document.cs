using Atlas.Domain.Enums;

namespace Atlas.Domain.Entities;

/// <summary>
/// Represents an identity document or selfie submitted with the application.
/// </summary>
public class Document
{
    private Document() { } // For EF Core

    public Document(Guid applicationId, DocumentType type, byte[] imageData, DateTime uploadedAt)
    {
        Id = Guid.NewGuid();
        ApplicationId = applicationId;
        Type = type;
        ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
        UploadedAt = uploadedAt;
    }

    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public DocumentType Type { get; private set; }
    public byte[] ImageData { get; private set; } = Array.Empty<byte>();
    public DateTime UploadedAt { get; private set; }
}
