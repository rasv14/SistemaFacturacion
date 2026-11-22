namespace Comercial360.Application.Sales;

public abstract class SaleRegistrationException(string message) : Exception(message);

public sealed class CustomerNotFoundException()
    : SaleRegistrationException("No se encontró el cliente solicitado.");

public sealed class ProductNotFoundException(Guid productId)
    : SaleRegistrationException($"No se encontró el producto {productId}.");

public sealed class InactiveProductException(Guid productId)
    : SaleRegistrationException($"El producto {productId} está inactivo.");

public sealed class InsufficientStockException(Guid productId)
    : SaleRegistrationException($"No hay stock suficiente para el producto {productId}.");

public sealed class CatalogChangedException(Guid productId)
    : SaleRegistrationException($"El precio, estado o stock del producto {productId} cambió durante el registro. Recargue el catálogo e intente nuevamente.");

public sealed class IdempotencyConflictException()
    : SaleRegistrationException("La clave de idempotencia ya se utilizó con un contenido diferente.");

public sealed class TaxConfigurationException()
    : SaleRegistrationException("No está configurada la tasa tributaria requerida para calcular la venta.");
