using Comercial360.Application.Sales;
using Microsoft.EntityFrameworkCore;

namespace Comercial360.Adapters.Persistence;

public sealed class EfSaleCatalog(Comercial360DbContext dbContext) : ISaleCatalog
{
    public async Task<SaleCatalogSnapshot> LoadAsync(
        Guid customerId,
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var customerExists = await dbContext.Customers
            .AnyAsync(customer => customer.Id == customerId && customer.IsActive, cancellationToken);
        var products = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .Select(product => new ProductSnapshot(
                product.Id,
                product.Description,
                product.UnitNetPrice,
                product.Stock,
                product.IsActive))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        return new SaleCatalogSnapshot(customerExists, products);
    }
}
