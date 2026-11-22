using System.ComponentModel.DataAnnotations;

namespace Comercial360.Api.Contracts.Sales;

public sealed class RegisterSaleRequest : IValidatableObject
{
    [Required]
    public Guid CustomerId { get; init; }

    [Required]
    [RegularExpression("^(Invoice|Receipt)$")]
    public string DocumentType { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public IReadOnlyCollection<RegisterSaleItemRequest> Items { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CustomerId == Guid.Empty)
        {
            yield return new ValidationResult("CustomerId no puede estar vacío.", [nameof(CustomerId)]);
        }

        if (Items is null)
        {
            yield return new ValidationResult("Debe especificar los productos de la venta.", [nameof(Items)]);
            yield break;
        }

        for (var index = 0; index < Items.Count; index++)
        {
            var item = Items.ElementAt(index);
            if (item.ProductId == Guid.Empty)
            {
                yield return new ValidationResult($"El producto en la posición {index} no es válido.", [nameof(Items)]);
            }

            if (item.Quantity <= 0 || item.Quantity > 1000000)
            {
                yield return new ValidationResult($"La cantidad en la posición {index} debe ser mayor que cero.", [nameof(Items)]);
            }
            else if (decimal.Round(item.Quantity, 4) != item.Quantity)
            {
                yield return new ValidationResult($"La cantidad en la posición {index} admite como máximo cuatro decimales.", [nameof(Items)]);
            }
        }
    }
}

public sealed class RegisterSaleItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(typeof(decimal), "0.0001", "1000000")]
    public decimal Quantity { get; init; }
}
