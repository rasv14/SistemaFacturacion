namespace Comercial360.Application.Sales;

public sealed record SaleLineData(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitNetPrice,
    decimal NetAmount,
    decimal TaxAmount);

public sealed record SaleSummaryData(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset CreatedAtUtc,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    string Status);

public sealed record SaleDetailData(
    Guid Id,
    Guid CustomerId,
    DateTimeOffset CreatedAtUtc,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    string Status,
    IReadOnlyCollection<SaleLineData> Items);

public sealed record SalePageData(IReadOnlyCollection<SaleSummaryData> Items, int Page, int PageSize, int TotalCount);

public interface ISaleQuery
{
    Task<SalePageData> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<SaleDetailData?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
