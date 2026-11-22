using Comercial360.Application.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Comercial360.Api.Adapters.In.Http;

[ApiController]
[Route("api/electronic-documents")]
public sealed class ElectronicDocumentsController(IElectronicDocumentQuery documentQuery) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ElectronicDocumentStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ElectronicDocumentStatusResult>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await documentQuery.GetStatusAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
