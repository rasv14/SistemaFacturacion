using Comercial360.Application.Sales;
using Comercial360.Application.Catalog;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Comercial360.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            CustomerNotFoundException or ProductNotFoundException =>
                (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
            InsufficientStockException or CatalogChangedException or IdempotencyConflictException =>
                (StatusCodes.Status409Conflict, "Conflicto de operación", exception.Message),
            DuplicateCatalogValueException =>
                (StatusCodes.Status409Conflict, "Conflicto de catálogo", exception.Message),
            InactiveProductException =>
                (StatusCodes.Status422UnprocessableEntity, "Regla de negocio no satisfecha", exception.Message),
            TaxConfigurationException =>
                (StatusCodes.Status503ServiceUnavailable, "Configuración incompleta", exception.Message),
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),
            _ =>
                (StatusCodes.Status500InternalServerError, "Error interno", "Ocurrió un error inesperado.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado al procesar {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["correlationId"] =
            httpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString() ?? httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
