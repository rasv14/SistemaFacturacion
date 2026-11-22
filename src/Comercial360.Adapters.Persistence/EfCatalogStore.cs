using Comercial360.Application.Catalog;
using Comercial360.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
    //comentario
namespace Comercial360.Adapters.Persistence;

public sealed class EfCatalogStore(Comercial360DbContext dbContext) : ICatalogStore
{
    public async Task<IReadOnlyCollection<Customer>> ListCustomersAsync(
        string? search,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(customer => customer.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(customer =>
                customer.Name.Contains(search) ||
                customer.DocumentNumber.Contains(search) ||
                customer.DocumentType.Contains(search));
        }

        return await query
            .OrderBy(customer => customer.Name)
            .ThenBy(customer => customer.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Customer?> GetCustomerAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<Customer?> GetCustomerByDocumentAsync(
        string documentNumber,
        string? documentType,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Customers.AsNoTracking().Where(customer =>
            customer.DocumentNumber == documentNumber);

        if (!string.IsNullOrWhiteSpace(documentType))
        {
            query = query.Where(customer => customer.DocumentType == documentType);
        }

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveCustomerAsync(Customer customer, bool isNew, CancellationToken cancellationToken)
    {
        if (isNew)
        {
            dbContext.Customers.Add(customer);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateCatalogValueException(
                "Ya existe un cliente con el mismo tipo y número de documento.");
        }
    }

    public async Task<IReadOnlyCollection<Product>> ListProductsAsync(
        string? search,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking().AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(product => product.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.Code.Contains(search) ||
                product.Description.Contains(search));
        }

        return await query
            .OrderBy(product => product.Description)
            .ThenBy(product => product.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task SaveProductAsync(Product product, bool isNew, CancellationToken cancellationToken)
    {
        if (isNew)
        {
            dbContext.Products.Add(product);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw new DuplicateCatalogValueException("Ya existe un producto con el mismo código.");
        }
    }

    private static bool IsUniqueConstraintViolation(Exception exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
