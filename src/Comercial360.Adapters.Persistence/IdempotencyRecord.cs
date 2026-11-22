namespace Comercial360.Adapters.Persistence;

public sealed class IdempotencyRecord
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public Guid SaleId { get; set; }
    public Guid ElectronicDocumentId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
