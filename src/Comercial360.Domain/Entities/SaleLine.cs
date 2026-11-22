namespace Comercial360.Domain.Entities;

public sealed class SaleLine
{
    private SaleLine() { }

    private SaleLine(Guid id, Guid productId, string description, decimal quantity, decimal unitNetPrice, decimal netAmount, decimal taxAmount)
    {
        Id = id;
        ProductId = productId;
        DescriptionSnapshot = description;
        Quantity = quantity;
        UnitNetPrice = unitNetPrice;
        NetAmount = netAmount;
        TaxAmount = taxAmount;
    }

    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string DescriptionSnapshot { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitNetPrice { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public Sale Sale { get; private set; } = null!;

    public static SaleLine Create(Guid productId, string description, decimal quantity, decimal unitNetPrice, decimal taxAmount)
    {
        if (productId == Guid.Empty || string.IsNullOrWhiteSpace(description) || quantity <= 0 || unitNetPrice < 0 || taxAmount < 0)
        {
            throw new ArgumentException("Los datos de la línea de venta no son válidos.");
        }

        var netAmount = decimal.Round(quantity * unitNetPrice, 2, MidpointRounding.AwayFromZero);
        return new SaleLine(Guid.NewGuid(), productId, description.Trim(), quantity, unitNetPrice, netAmount, taxAmount);
    }
}
