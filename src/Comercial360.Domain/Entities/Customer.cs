namespace Comercial360.Domain.Entities;

public sealed class Customer
{
    private Customer() { }

    public Guid Id { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public static Customer Create(string documentType, string documentNumber, string name)
    {
        var customer = new Customer { Id = Guid.NewGuid() };
        customer.Update(documentType, documentNumber, name, true);
        return customer;
    }

    public void Update(string documentType, string documentNumber, string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(documentType) ||
            string.IsNullOrWhiteSpace(documentNumber) ||
            string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tipo y número de documento y nombre son obligatorios.");
        }

        DocumentType = documentType.Trim();
        DocumentNumber = documentNumber.Trim();
        Name = name.Trim();
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
}
