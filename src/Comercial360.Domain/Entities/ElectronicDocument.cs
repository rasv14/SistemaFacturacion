namespace Comercial360.Domain.Entities;

public enum ElectronicDocumentStatus
{
    Pending,
    Submitted,
    Accepted,
    AcceptedWithObservations,
    Rejected,
    TechnicalError,
    NeedsReconciliation
}

public sealed class ElectronicDocument
{
    private ElectronicDocument() { }

    private ElectronicDocument(Guid id, Guid saleId, string documentType, DateTimeOffset createdAtUtc)
    {
        Id = id;
        SaleId = saleId;
        DocumentType = documentType;
        Status = ElectronicDocumentStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string? Series { get; private set; }
    public string? Number { get; private set; }
    public ElectronicDocumentStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? NormalizedMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? LastAttemptAtUtc { get; private set; }
    public Sale Sale { get; private set; } = null!;

    public static ElectronicDocument Create(Guid saleId, string documentType, DateTimeOffset createdAtUtc)
    {
        if (saleId == Guid.Empty || string.IsNullOrWhiteSpace(documentType))
        {
            throw new ArgumentException("El documento electrónico requiere una venta y un tipo.");
        }

        return new ElectronicDocument(Guid.NewGuid(), saleId, documentType, createdAtUtc.ToUniversalTime());
    }
}
