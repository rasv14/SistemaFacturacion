using Comercial360.Application.Sales;
using Comercial360.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Comercial360.Adapters.Persistence;

public sealed class EfSaleRegistrationStore(Comercial360DbContext dbContext) : ISaleRegistrationStore
{
    public async Task<RegisterSaleResult?> FindByIdempotencyKeyAsync(
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var record = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.IdempotencyKey == idempotencyKey, cancellationToken);
        return record is null ? null : await ToResultAsync(record, requestHash, cancellationToken);
    }

    public async Task<RegisterSaleResult> RegisterAsync(
        Sale sale,
        ElectronicDocument electronicDocument,
        IReadOnlyCollection<StockMovement> stockMovements,
        OutboxMessage outboxMessage,
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var existing = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return await ToResultAsync(existing, requestHash, cancellationToken);
        }

        foreach (var line in sale.Lines)
        {
            var affectedRows = await dbContext.Products
                .Where(product =>
                    product.Id == line.ProductId &&
                    product.IsActive &&
                    product.UnitNetPrice == line.UnitNetPrice &&
                    product.Stock >= line.Quantity)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(product => product.Stock, product => product.Stock - line.Quantity),
                    cancellationToken);

            if (affectedRows != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new CatalogChangedException(line.ProductId);
            }
        }

        dbContext.Sales.Add(sale);
        dbContext.ElectronicDocuments.Add(electronicDocument);
        dbContext.StockMovements.AddRange(stockMovements);
        dbContext.OutboxMessages.Add(outboxMessage);
        dbContext.IdempotencyRecords.Add(new IdempotencyRecord
        {
            IdempotencyKey = idempotencyKey,
            RequestHash = requestHash,
            SaleId = sale.Id,
            ElectronicDocumentId = electronicDocument.Id,
            CreatedAtUtc = sale.CreatedAtUtc
        });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (IsUniqueConstraintViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            var concurrentRecord = await dbContext.IdempotencyRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.IdempotencyKey == idempotencyKey, cancellationToken);
            if (concurrentRecord is null)
            {
                throw;
            }

            return await ToResultAsync(concurrentRecord, requestHash, cancellationToken);
        }

        return new RegisterSaleResult(
            sale.Id,
            electronicDocument.Id,
            sale.Status,
            electronicDocument.Status.ToString(),
            false);
    }

    private async Task<RegisterSaleResult> ToResultAsync(
        IdempotencyRecord record,
        string requestHash,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new IdempotencyConflictException();
        }

        var saleStatus = await dbContext.Sales
            .AsNoTracking()
            .Where(sale => sale.Id == record.SaleId)
            .Select(sale => sale.Status)
            .SingleAsync(cancellationToken);
        var documentStatus = await dbContext.ElectronicDocuments
            .AsNoTracking()
            .Where(document => document.Id == record.ElectronicDocumentId)
            .Select(document => document.Status)
            .SingleAsync(cancellationToken);

        return new RegisterSaleResult(
            record.SaleId,
            record.ElectronicDocumentId,
            saleStatus,
            documentStatus.ToString(),
            true);
    }

    private static bool IsUniqueConstraintViolation(Exception exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
