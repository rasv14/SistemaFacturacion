using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Comercial360.Api.Contracts.Sales;
using Comercial360.Api.Infrastructure;
using Comercial360.Application.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Comercial360.Api.Adapters.In.Http;

[ApiController]
[Route("api/sales")]
public sealed class SalesController(
    ISaleRegistrationService registrationService,
    ISaleQuery saleQuery,
    ILogger<SalesController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SalePageData), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalePageData>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return Problem(
                title: "Paginación inválida",
                detail: "page debe ser mayor que cero y pageSize debe estar entre 1 y 100.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Ok(await saleQuery.ListAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDetailData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleDetailData>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var sale = await saleQuery.GetByIdAsync(id, cancellationToken);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RegisterSaleResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RegisterSaleResponse>> Register(
        [FromBody] RegisterSaleRequest request,
        [FromHeader(Name = "Idempotency-Key"), Required] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 128)
        {
            return Problem(
                title: "Idempotency-Key inválida",
                detail: "Envíe el encabezado Idempotency-Key con entre 1 y 128 caracteres.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        var correlationId = HttpContext.Items[CorrelationIdMiddleware.ItemKey]?.ToString()
            ?? HttpContext.TraceIdentifier;
        var command = new RegisterSaleCommand(
            request.CustomerId,
            request.DocumentType,
            request.Items.Select(item => new RegisterSaleItem(item.ProductId, item.Quantity)).ToArray(),
            idempotencyKey,
            userId,
            correlationId);

        var result = await registrationService.HandleAsync(command, cancellationToken);
        logger.LogInformation(
            "Sale {SaleId} registered with electronic document {ElectronicDocumentId}; replay={IsReplay}",
            result.SaleId,
            result.ElectronicDocumentId,
            result.IsReplay);

        var response = new RegisterSaleResponse(
            result.SaleId,
            result.SaleStatus,
            result.ElectronicDocumentId,
            result.ElectronicDocumentStatus,
            $"/api/electronic-documents/{result.ElectronicDocumentId}",
            result.IsReplay);

        return Accepted(response.StatusUrl, response);
    }
}
