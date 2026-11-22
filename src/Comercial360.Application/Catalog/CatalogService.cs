using Comercial360.Domain.Entities;

namespace Comercial360.Application.Catalog;

public sealed class CatalogService(ICatalogStore store) : ICatalogService
{
    public async Task<IReadOnlyCollection<CustomerData>> ListCustomersAsync(
        string? search,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePage(page, pageSize);
        var customers = await store.ListCustomersAsync(
            search?.Trim(),
            includeInactive,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        return customers.Select(ToData).ToArray();
    }

    public async Task<CustomerData?> GetCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await store.GetCustomerAsync(id, cancellationToken);
        return customer is null ? null : ToData(customer);
    }

    public async Task<CustomerData?> GetCustomerByDocumentAsync(
        string documentNumber,
        string? documentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            throw new ArgumentException("El número de documento es obligatorio.", nameof(documentNumber));
        }

        var customer = await store.GetCustomerByDocumentAsync(
            documentNumber.Trim(),
            string.IsNullOrWhiteSpace(documentType) ? null : documentType.Trim(),
            cancellationToken);
        return customer is null ? null : ToData(customer);
    }

    public async Task<CustomerData> CreateCustomerAsync(CustomerInput input, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(input.DocumentType, input.DocumentNumber, input.Name);
        await store.SaveCustomerAsync(customer, true, cancellationToken);
        return ToData(customer);
    }

    public async Task<CustomerData?> UpdateCustomerAsync(
        Guid id,
        CustomerInput input,
        CancellationToken cancellationToken)
    {
        var customer = await store.GetCustomerAsync(id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.Update(input.DocumentType, input.DocumentNumber, input.Name, input.IsActive);
        await store.SaveCustomerAsync(customer, false, cancellationToken);
        return ToData(customer);
    }

    public async Task<bool> DeactivateCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await store.GetCustomerAsync(id, cancellationToken);
        if (customer is null)
        {
            return false;
        }

        customer.Deactivate();
        await store.SaveCustomerAsync(customer, false, cancellationToken);
        return true;
    }

    public async Task<IReadOnlyCollection<ProductData>> ListProductsAsync(
        string? search,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ValidatePage(page, pageSize);
        var products = await store.ListProductsAsync(
            search?.Trim(),
            includeInactive,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        return products.Select(ToData).ToArray();
    }

    public async Task<ProductData?> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(id, cancellationToken);
        return product is null ? null : ToData(product);
    }

    public async Task<ProductData> CreateProductAsync(ProductInput input, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            input.Code,
            input.Description,
            input.UnitNetPrice,
            input.InitialStock ?? 0m);
        await store.SaveProductAsync(product, true, cancellationToken);
        return ToData(product);
    }

    public async Task<ProductData?> UpdateProductAsync(
        Guid id,
        ProductInput input,
        CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Update(input.Code, input.Description, input.UnitNetPrice, input.IsActive);
        await store.SaveProductAsync(product, false, cancellationToken);
        return ToData(product);
    }

    public async Task<bool> DeactivateProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await store.GetProductAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        product.Deactivate();
        await store.SaveProductAsync(product, false, cancellationToken);
        return true;
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "La página debe ser mayor que cero y pageSize debe estar entre 1 y 100.");
        }
    }

    private static CustomerData ToData(Customer customer) =>
        new(customer.Id, customer.DocumentType, customer.DocumentNumber, customer.Name, customer.IsActive);

    private static ProductData ToData(Product product) =>
        new(product.Id, product.Code, product.Description, product.UnitNetPrice, product.Stock, product.IsActive);
}
