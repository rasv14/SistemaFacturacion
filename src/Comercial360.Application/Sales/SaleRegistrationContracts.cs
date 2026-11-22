using Comercial360.Domain.Entities;

namespace Comercial360.Application.Sales;

public sealed record RegisterSaleItem(Guid ProductId, decimal Quantity);

public sealed record RegisterSaleCommand(
    Guid CustomerId,
    string DocumentType,
    IReadOnlyCollection<RegisterSaleItem> Items,
    string IdempotencyKey,
    string RequestedBy,
    string CorrelationId);

public sealed record ProductSnapshot(Guid Id, string Description, decimal UnitNetPrice, decimal Stock, bool IsActive);

public sealed record SaleCatalogSnapshot(bool CustomerExists, IReadOnlyDictionary<Guid, ProductSnapshot> Products);

public sealed record RegisterSaleResult(
    Guid SaleId,
    Guid ElectronicDocumentId,
    string SaleStatus,
    string ElectronicDocumentStatus,
    bool IsReplay);

public sealed record TaxConfiguration(decimal? RatePercent);

public sealed record ElectronicDocumentStatusResult(
    Guid Id,
    Guid SaleId,
    string DocumentType,
    string Status,
    string? ExternalReference,
    string? Message,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAttemptAtUtc);

public interface ISaleCatalog
{
    Task<SaleCatalogSnapshot> LoadAsync(
        Guid customerId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}

public interface ISaleRegistrationStore
{
    Task<RegisterSaleResult?> FindByIdempotencyKeyAsync(
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken);

    Task<RegisterSaleResult> RegisterAsync(
        Sale sale,
        ElectronicDocument electronicDocument,
        IReadOnlyCollection<StockMovement> stockMovements,
        OutboxMessage outboxMessage,
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken);
}

public interface IElectronicDocumentQuery
{
    Task<ElectronicDocumentStatusResult?> GetStatusAsync(Guid id, CancellationToken cancellationToken);
}

public interface ISaleRegistrationService
{
    Task<RegisterSaleResult> HandleAsync(RegisterSaleCommand command, CancellationToken cancellationToken);
}
