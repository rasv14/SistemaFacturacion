namespace Comercial360.Domain.Entities;

public sealed class Sale
{
    private Sale() { }

    private Sale(Guid id, Guid customerId, string requestedBy, DateTimeOffset createdAtUtc, IReadOnlyCollection<SaleLine> lines)
    {
        Id = id;
        CustomerId = customerId;
        RequestedBy = requestedBy;
        CreatedAtUtc = createdAtUtc;
        Lines = lines.ToList();
        Subtotal = Lines.Sum(line => line.NetAmount);
        TaxTotal = Lines.Sum(line => line.TaxAmount);
        Total = Subtotal + TaxTotal;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal Total { get; private set; }
    public string Status { get; private set; } = "Registered";
    public ICollection<SaleLine> Lines { get; private set; } = new List<SaleLine>();

    public static Sale Create(
        Guid customerId,
        string requestedBy,
        DateTimeOffset createdAtUtc,
        IEnumerable<SaleLine> lines)
    {
        var saleLines = lines.ToList();
        if (customerId == Guid.Empty || saleLines.Count == 0)
        {
            throw new ArgumentException("La venta requiere un cliente y al menos una línea.");
        }

        return new Sale(Guid.NewGuid(), customerId, requestedBy, createdAtUtc.ToUniversalTime(), saleLines);
    }
}
