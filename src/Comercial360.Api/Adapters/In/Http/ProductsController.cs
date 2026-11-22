using Comercial360.Api.Contracts.Catalog;
using Comercial360.Application.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace Comercial360.Api.Adapters.In.Http;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PageResponse<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
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

        var products = await catalogService.ListProductsAsync(
            search, includeInactive, page, pageSize, cancellationToken);
        return Ok(new PageResponse<ProductResponse>(products.Select(ToResponse).ToArray(), page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await catalogService.GetProductAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(ToResponse(product));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await catalogService.CreateProductAsync(
            new ProductInput(
                request.Code,
                request.Description,
                request.UnitNetPrice,
                request.InitialStock,
                true),
            cancellationToken);
        var response = ToResponse(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await catalogService.UpdateProductAsync(
            id,
            new ProductInput(
                request.Code,
                request.Description,
                request.UnitNetPrice,
                null,
                request.IsActive),
            cancellationToken);
        return product is null ? NotFound() : Ok(ToResponse(product));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deactivated = await catalogService.DeactivateProductAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }

    private static ProductResponse ToResponse(ProductData product) =>
        new(product.Id, product.Code, product.Description, product.UnitNetPrice, product.Stock, product.IsActive);
}
