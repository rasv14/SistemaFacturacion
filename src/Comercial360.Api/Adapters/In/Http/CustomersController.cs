using Comercial360.Api.Contracts.Catalog;
using Comercial360.Application.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace Comercial360.Api.Adapters.In.Http;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PageResponse<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PageResponse<CustomerResponse>>> GetAll(
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

        var customers = await catalogService.ListCustomersAsync(
            search, includeInactive, page, pageSize, cancellationToken);
        return Ok(new PageResponse<CustomerResponse>(customers.Select(ToResponse).ToArray(), page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await catalogService.GetCustomerAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(ToResponse(customer));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await catalogService.CreateCustomerAsync(
            new CustomerInput(request.DocumentType, request.DocumentNumber, request.Name, true),
            cancellationToken);
        var response = ToResponse(customer);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await catalogService.UpdateCustomerAsync(
            id,
            new CustomerInput(request.DocumentType, request.DocumentNumber, request.Name, request.IsActive),
            cancellationToken);
        return customer is null ? NotFound() : Ok(ToResponse(customer));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deactivated = await catalogService.DeactivateCustomerAsync(id, cancellationToken);
        return deactivated ? NoContent() : NotFound();
    }

    private static CustomerResponse ToResponse(CustomerData customer) =>
        new(customer.Id, customer.DocumentType, customer.DocumentNumber, customer.Name, customer.IsActive);
}
