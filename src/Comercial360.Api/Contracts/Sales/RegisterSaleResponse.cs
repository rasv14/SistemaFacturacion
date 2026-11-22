namespace Comercial360.Api.Contracts.Sales;

public sealed record RegisterSaleResponse(
    Guid SaleId,
    string SaleStatus,
    Guid ElectronicDocumentId,
    string ElectronicDocumentStatus,
    string StatusUrl,
    bool IsReplay);
