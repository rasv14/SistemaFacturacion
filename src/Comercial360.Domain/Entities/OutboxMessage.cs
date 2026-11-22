namespace Comercial360.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    public OutboxMessage(Guid electronicDocumentId, string payload, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        ElectronicDocumentId = electronicDocumentId;
        EventType = "ElectronicDocumentRequested";
        Payload = payload;
        Status = "Pending";
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid ElectronicDocumentId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int Attempts { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
}
