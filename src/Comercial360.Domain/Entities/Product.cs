namespace Comercial360.Domain.Entities;

public sealed class Product
{
    private Product() { }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal UnitNetPrice { get; private set; }
    public decimal Stock { get; private set; }
    public bool IsActive { get; private set; }

    public static Product Create(string code, string description, decimal unitNetPrice, decimal initialStock)
    {
        if (initialStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialStock));
        }

        var product = new Product { Id = Guid.NewGuid(), Stock = initialStock };
        product.Update(code, description, unitNetPrice, true);
        return product;
    }

    public void Update(string code, string description, decimal unitNetPrice, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(description) ||
            unitNetPrice < 0)
        {
            throw new ArgumentException("Código y descripción son obligatorios; el precio no puede ser negativo.");
        }

        Code = code.Trim();
        Description = description.Trim();
        UnitNetPrice = unitNetPrice;
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
}
