using System.ComponentModel.DataAnnotations;

namespace Comercial360.Api.Contracts.Catalog;

/// <summary>Datos necesarios para crear un producto.</summary>
public sealed record CreateProductRequest
{
    /// <summary>Código único de catálogo.</summary>
    [Required, MaxLength(50)]
    public required string Code { get; init; }

    /// <summary>Descripción del producto.</summary>
    [Required, MaxLength(300)]
    public required string Description { get; init; }

    /// <summary>Precio neto unitario administrado por el servidor.</summary>
    [Range(typeof(decimal), "0", "99999999999999")]
    public decimal UnitNetPrice { get; init; }

    /// <summary>Existencia inicial al crear el producto.</summary>
    [Range(typeof(decimal), "0", "99999999999999")]
    public decimal InitialStock { get; init; }
}

/// <summary>Datos editables de un producto; el stock se conserva.</summary>
public sealed record UpdateProductRequest
{
    /// <summary>Código único de catálogo.</summary>
    [Required, MaxLength(50)]
    public required string Code { get; init; }

    /// <summary>Descripción del producto.</summary>
    [Required, MaxLength(300)]
    public required string Description { get; init; }

    /// <summary>Precio neto unitario.</summary>
    [Range(typeof(decimal), "0", "99999999999999")]
    public decimal UnitNetPrice { get; init; }

    /// <summary>Indica si puede incluirse en nuevas ventas.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Representación pública de un producto.</summary>
public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    decimal UnitNetPrice,
    decimal Stock,
    bool IsActive);

/// <summary>Resultados de una página y los parámetros usados para solicitarla.</summary>
public sealed record PageResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize);
