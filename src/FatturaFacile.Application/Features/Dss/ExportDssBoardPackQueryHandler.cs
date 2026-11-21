// <copyright file="ExportDssBoardPackQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Dss;
using MediatR;

namespace FatturaFacile.Application.Features.Dss;

/// <summary>
/// Handler for <see cref="ExportDssBoardPackQuery"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
public sealed class ExportDssBoardPackQueryHandler(IDssApiClient api) : IRequestHandler<ExportDssBoardPackQuery, DssBoardPack>
{
    /// <inheritdoc/>
    public Task<DssBoardPack> Handle(ExportDssBoardPackQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return ExportAsync(request, cancellationToken);
    }

    private async Task<DssBoardPack> ExportAsync(ExportDssBoardPackQuery request, CancellationToken cancellationToken)
    {
        var content = await api.ExportBoardPackAsync(request.Filter, request.Format, request.Locale, cancellationToken).ConfigureAwait(false);
        var name = $"board-pack-{request.Filter.PeriodPreset.ToLowerInvariant()}-{request.Filter.Currency.ToLowerInvariant()}.{request.Format}";

        return new DssBoardPack(content, name);
    }
}
