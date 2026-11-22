using System.ComponentModel.DataAnnotations;

namespace Comercial360.Api.Contracts.Catalog;

/// <summary>Datos necesarios para crear un cliente.</summary>
public sealed record CreateCustomerRequest
{
    /// <summary>Tipo de documento del cliente.</summary>
    [Required, MaxLength(20)]
    public required string DocumentType { get; init; }

    /// <summary>Número de documento del cliente.</summary>
    [Required, MaxLength(20)]
    public required string DocumentNumber { get; init; }

    /// <summary>Nombre o razón social.</summary>
    [Required, MaxLength(200)]
    public required string Name { get; init; }
}

/// <summary>Datos para reemplazar los datos editables de un cliente.</summary>
public sealed record UpdateCustomerRequest
{
    /// <summary>Tipo de documento del cliente.</summary>
    [Required, MaxLength(20)]
    public required string DocumentType { get; init; }

    /// <summary>Número de documento del cliente.</summary>
    [Required, MaxLength(20)]
    public required string DocumentNumber { get; init; }

    /// <summary>Nombre o razón social.</summary>
    [Required, MaxLength(200)]
    public required string Name { get; init; }

    /// <summary>Indica si el cliente puede usarse en nuevas ventas.</summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>Representación pública de un cliente.</summary>
public sealed record CustomerResponse(
    Guid Id,
    string DocumentType,
    string DocumentNumber,
    string Name,
    bool IsActive);
