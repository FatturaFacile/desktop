// <copyright file="ConvertServiceReportToInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="ConvertServiceReportToInvoiceCommand"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="time">The clock.</param>
public sealed class ConvertServiceReportToInvoiceCommandHandler(IFieldServiceApiClient api, TimeProvider time) : IRequestHandler<ConvertServiceReportToInvoiceCommand, FieldServiceCallResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceCallResult> Handle(ConvertServiceReportToInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return api.IsAuthenticated
            ? FieldServiceCallRunner.RunAsync(() => ConvertAsync(request, cancellationToken), cancellationToken)
            : Task.FromResult(new FieldServiceCallResult(FieldServiceCallStatus.Unauthorized));
    }

    private async Task<FieldServiceCallResult> ConvertAsync(ConvertServiceReportToInvoiceCommand request, CancellationToken cancellationToken)
    {
        var route = "service-reports/" + Uri.EscapeDataString(request.ReportId) + "/invoice";
        var reply = await api.SendAsync(HttpMethod.Post, route, FieldServiceRequests.Invoice(request.TaxRate, request.DueDate), cancellationToken).ConfigureAwait(false);
        var status = FieldServiceCallRunner.Classify(reply.Status);
        if (status != FieldServiceCallStatus.Ok)
        {
            return new FieldServiceCallResult(status, FieldServiceErrorReader.Describe(reply.Body));
        }

        using var document = JsonDocument.Parse(reply.Body);
        var data = FieldServiceJson.Child(document.RootElement, "data") ?? default;

        return new FieldServiceCallResult(status, AtUtc: time.GetUtcNow().UtcDateTime, Id: FieldServiceJson.NullableText(data, "invoice_id"));
    }
}
