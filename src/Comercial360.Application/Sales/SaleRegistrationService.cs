using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Comercial360.Domain.Entities;

namespace Comercial360.Application.Sales;

public sealed class SaleRegistrationService(
    ISaleCatalog catalog,
    ISaleRegistrationStore store,
    TaxConfiguration taxConfiguration,
    TimeProvider timeProvider) : ISaleRegistrationService
{
    public async Task<RegisterSaleResult> HandleAsync(RegisterSaleCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            throw new ArgumentException("Debe enviarse el encabezado Idempotency-Key.");
        }

        if (string.IsNullOrWhiteSpace(command.RequestedBy))
        {
            throw new ArgumentException("No fue posible identificar al usuario autenticado.");
        }

        var normalizedItems = command.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new RegisterSaleItem(group.Key, group.Sum(item => item.Quantity)))
            .OrderBy(item => item.ProductId)
            .ToArray();

        var requestHash = ComputeRequestHash(command.CustomerId, command.DocumentType, normalizedItems);
        var previousResult = await store.FindByIdempotencyKeyAsync(
            command.IdempotencyKey,
            requestHash,
            cancellationToken);
        if (previousResult is not null)
        {
            return previousResult with { IsReplay = true };
        }

        var taxRatePercent = taxConfiguration.RatePercent;
        if (taxRatePercent is null or < 0 or > 100)
        {
            throw new TaxConfigurationException();
        }

        var snapshot = await catalog.LoadAsync(
            command.CustomerId,
            normalizedItems.Select(item => item.ProductId).ToArray(),
            cancellationToken);

        if (!snapshot.CustomerExists)
        {
            throw new CustomerNotFoundException();
        }

        var now = timeProvider.GetUtcNow();
        var saleLines = new List<SaleLine>(normalizedItems.Length);
        var movements = new List<StockMovement>(normalizedItems.Length);
        foreach (var item in normalizedItems)
        {
            if (!snapshot.Products.TryGetValue(item.ProductId, out var product))
            {
                throw new ProductNotFoundException(item.ProductId);
            }

            if (!product.IsActive)
            {
                throw new InactiveProductException(item.ProductId);
            }

            if (product.Stock < item.Quantity)
            {
                throw new InsufficientStockException(item.ProductId);
            }

            var netAmount = decimal.Round(item.Quantity * product.UnitNetPrice, 2, MidpointRounding.AwayFromZero);
            var taxAmount = decimal.Round(netAmount * taxRatePercent.Value / 100m, 2, MidpointRounding.AwayFromZero);
            saleLines.Add(SaleLine.Create(item.ProductId, product.Description, item.Quantity, product.UnitNetPrice, taxAmount));
        }

        var sale = Sale.Create(command.CustomerId, command.RequestedBy, now, saleLines);
        var electronicDocument = ElectronicDocument.Create(sale.Id, command.DocumentType, now);
        foreach (var item in normalizedItems)
        {
            movements.Add(new StockMovement(item.ProductId, sale.Id, item.Quantity, now));
        }

        var outboxPayload = JsonSerializer.Serialize(new
        {
            electronicDocumentId = electronicDocument.Id,
            correlationId = command.CorrelationId
        });
        var outboxMessage = new OutboxMessage(electronicDocument.Id, outboxPayload, now);

        return await store.RegisterAsync(
            sale,
            electronicDocument,
            movements,
            outboxMessage,
            command.IdempotencyKey,
            requestHash,
            cancellationToken);
    }

    private static string ComputeRequestHash(
        Guid customerId,
        string documentType,
        IReadOnlyCollection<RegisterSaleItem> items)
    {
        var canonicalRequest = JsonSerializer.Serialize(new
        {
            customerId,
            documentType,
            items = items.Select(item => new { item.ProductId, item.Quantity })
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)));
    }
}
