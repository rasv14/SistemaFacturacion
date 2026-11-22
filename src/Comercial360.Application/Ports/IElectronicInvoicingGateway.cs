namespace Comercial360.Application.Ports;

public sealed record ElectronicDocumentPayload(Guid ElectronicDocumentId, string CorrelationId, string ContentXml);

public sealed record ElectronicSubmissionResult(
    string ProviderStatus,
    bool IsAccepted,
    bool IsRejected,
    string? ExternalReference,
    string? ResponseXml,
    string? Message);

public interface IElectronicInvoicingGateway
{
    Task<ElectronicSubmissionResult> SubmitAsync(
        ElectronicDocumentPayload payload,
        CancellationToken cancellationToken);
}
