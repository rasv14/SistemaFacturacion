using Comercial360.Application.Sales;
using Microsoft.EntityFrameworkCore;

namespace Comercial360.Adapters.Persistence;

public sealed class EfElectronicDocumentQuery(Comercial360DbContext dbContext) : IElectronicDocumentQuery
{
    public async Task<ElectronicDocumentStatusResult?> GetStatusAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await dbContext.ElectronicDocuments
            .AsNoTracking()
            .Where(document => document.Id == id)
            .Select(document => new
            {
                document.Id,
                document.SaleId,
                document.DocumentType,
                document.Status,
                document.ExternalReference,
                document.NormalizedMessage,
                document.CreatedAtUtc,
                document.LastAttemptAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        return document is null
            ? null
            : new ElectronicDocumentStatusResult(
                document.Id,
                document.SaleId,
                document.DocumentType,
                document.Status.ToString(),
                document.ExternalReference,
                document.NormalizedMessage,
                document.CreatedAtUtc,
                document.LastAttemptAtUtc);
    }
}
