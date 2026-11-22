namespace Comercial360.Domain.Entities;

public sealed class StockMovement
{
    private StockMovement() { }

    public StockMovement(Guid productId, Guid saleId, decimal quantity, DateTimeOffset createdAtUtc)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        SaleId = saleId;
        Quantity = quantity;
        Type = "Sale";
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid SaleId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
