namespace Comercial360.Application.Catalog;

public sealed record CustomerData(
    Guid Id,
    string DocumentType,
    string DocumentNumber,
    string Name,
    bool IsActive);

public sealed record ProductData(
    Guid Id,
    string Code,
    string Description,
    decimal UnitNetPrice,
    decimal Stock,
    bool IsActive);

public sealed record CustomerInput(string DocumentType, string DocumentNumber, string Name, bool IsActive);

public sealed record ProductInput(
    string Code,
    string Description,
    decimal UnitNetPrice,
    decimal? InitialStock,
    bool IsActive);

public interface ICatalogStore
{
    Task<IReadOnlyCollection<Domain.Entities.Customer>> ListCustomersAsync(
        string? search,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<Domain.Entities.Customer?> GetCustomerAsync(Guid id, CancellationToken cancellationToken);
    Task SaveCustomerAsync(Domain.Entities.Customer customer, bool isNew, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Domain.Entities.Product>> ListProductsAsync(
        string? search,
        bool includeInactive,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<Domain.Entities.Product?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    Task SaveProductAsync(Domain.Entities.Product product, bool isNew, CancellationToken cancellationToken);
}

public interface ICatalogService
{
    Task<IReadOnlyCollection<CustomerData>> ListCustomersAsync(
        string? search,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<CustomerData?> GetCustomerAsync(Guid id, CancellationToken cancellationToken);
    Task<CustomerData> CreateCustomerAsync(CustomerInput input, CancellationToken cancellationToken);
    Task<CustomerData?> UpdateCustomerAsync(Guid id, CustomerInput input, CancellationToken cancellationToken);
    Task<bool> DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProductData>> ListProductsAsync(
        string? search,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ProductData?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    Task<ProductData> CreateProductAsync(ProductInput input, CancellationToken cancellationToken);
    Task<ProductData?> UpdateProductAsync(Guid id, ProductInput input, CancellationToken cancellationToken);
    Task<bool> DeactivateProductAsync(Guid id, CancellationToken cancellationToken);
}
