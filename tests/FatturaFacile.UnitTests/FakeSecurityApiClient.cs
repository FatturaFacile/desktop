// <copyright file="FakeSecurityApiClient.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test double.

using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Scripted web-api security client that records what was sent.
/// </summary>
internal sealed class FakeSecurityApiClient : ISecurityApiClient
{
    public bool IsAuthenticated { get; set; } = true;

    public Func<AuditBatchRequest, AuditBatchOutcome> OnBatch { get; set; } = static _ => new AuditBatchOutcome(SyncSendResult.Success);

    public Func<PiiUnmaskRequest, PiiUnmaskOutcome> OnUnmask { get; set; } = static _ => new PiiUnmaskOutcome(SyncSendResult.Success, "secret-value");

    public List<AuditBatchRequest> Batches { get; } = [];

    public List<PiiUnmaskRequest> Unmasks { get; } = [];

    public Task<AuditBatchOutcome> UploadAuditBatchAsync(AuditBatchRequest request, CancellationToken cancellationToken)
    {
        Batches.Add(request);
        return Task.FromResult(OnBatch(request));
    }

    public Task<PiiUnmaskOutcome> UnmaskPiiAsync(PiiUnmaskRequest request, CancellationToken cancellationToken)
    {
        Unmasks.Add(request);
        return Task.FromResult(OnUnmask(request));
    }
}
