// <copyright file="ISecurityApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Client of the web-api security surface (offline audit batches and PII unmasking).
/// </summary>
public interface ISecurityApiClient
{
    /// <summary>
    /// Gets a value indicating whether a session token is available.
    /// </summary>
    public bool IsAuthenticated { get; }

    /// <summary>
    /// Uploads one batch of audit entries atomically (the server accepts or rejects the whole batch).
    /// </summary>
    /// <param name="request">The batch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome; never throws for HTTP or network failures.</returns>
    public Task<AuditBatchOutcome> UploadAuditBatchAsync(AuditBatchRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Requests the clear value of a masked field.
    /// </summary>
    /// <param name="request">The request with the mandatory justification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The classified outcome; never throws for HTTP or network failures.</returns>
    public Task<PiiUnmaskOutcome> UnmaskPiiAsync(PiiUnmaskRequest request, CancellationToken cancellationToken);
}
