// <copyright file="PrintVatRegisterCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="PrintVatRegisterCommand"/>: the definitive print (stampa definitiva) of a register. The lines printed are
/// locked, the pages continue the numbering of the year and the print is recorded in the audit chain.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="renderer">The PDF renderer.</param>
/// <param name="localization">The localization service.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="user">The signed-in user.</param>
/// <param name="audit">The local audit chain.</param>
/// <param name="time">The clock.</param>
public sealed class PrintVatRegisterCommandHandler(
    IUnitOfWork unitOfWork,
    ITabularPdfRenderer renderer,
    ILocalizationService localization,
    ICurrentTenantService tenant,
    ICurrentUserService user,
    ILocalAuditQueue audit,
    TimeProvider time) : IRequestHandler<PrintVatRegisterCommand, PrintVatRegisterResult>
{
    /// <inheritdoc/>
    public Task<PrintVatRegisterResult> Handle(PrintVatRegisterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<PrintVatRegisterResult> HandleInternalAsync(PrintVatRegisterCommand request, CancellationToken cancellationToken)
    {
        var kind = request.Kind;
        var section = request.Section;
        var year = request.Year;
        var from = request.From.Date;
        var to = request.To.Date;
        var registry = unitOfWork.Repository<VatRegisterEntry>();

        var series = await registry.FindAsync(e => e.Kind == kind && e.Section == section && e.Year == year, cancellationToken).ConfigureAwait(false);
        var printable = series.Where(e => !e.IsFinalized && e.RegistrationDate >= from && e.RegistrationDate <= to).OrderBy(static e => e.ProtocolNumber).ThenBy(static e => e.VatRate).ToList();
        if (printable.Count == 0)
        {
            throw new InvalidOperationException(VatComplianceErrors.NothingToPrint);
        }

        var first = printable[0].ProtocolNumber;
        var last = printable.Max(static e => e.ProtocolNumber);
        var lastPrinted = series.Where(static e => e.IsFinalized).Select(static e => e.ProtocolNumber).DefaultIfEmpty(0).Max();

        // Nothing earlier may stay unprinted, and the numbers printed continue the ones already printed.
        if (series.Any(e => !e.IsFinalized && e.ProtocolNumber < first))
        {
            throw new InvalidOperationException(VatComplianceErrors.PrintSequence);
        }

        var wanted = Enumerable.Range(lastPrinted + 1, last - lastPrinted).ToHashSet();
        if (wanted.Except(printable.Select(static e => e.ProtocolNumber)).Any())
        {
            throw new InvalidOperationException(VatComplianceErrors.ProtocolGap);
        }

        var previousRuns = await unitOfWork.Repository<VatRegisterPrintRun>().FindAsync(r => r.Kind == kind && r.Section == section && r.Year == year, cancellationToken).ConfigureAwait(false);
        var firstPage = previousRuns.Select(static r => r.LastPage).DefaultIfEmpty(0).Max() + 1;
        var report = VatRegisterReportBuilder.Build(kind, section, from, to, printable);
        var pages = VatRegisterNumbering.PagesFor(report.Lines.Count);
        var csv = VatRegisterCsvBuilder.Build(report);
        var pdf = await renderer.RenderAsync(VatRegisterTable.Build(report, key => localization[key], tenant.TenantName), firstPage, cancellationToken).ConfigureAwait(false);

        VatRegisterPrintRun run = new()
        {
            Kind = kind,
            Section = section,
            Year = year,
            FromDate = from,
            ToDate = to,
            FirstProtocol = first,
            LastProtocol = last,
            EntryCount = report.Lines.Count,
            FirstPage = firstPage,
            LastPage = firstPage + pages - 1,
            TotalTaxable = report.TotalTaxable,
            TotalVat = report.TotalVat,
            ContentSha256 = VatRegisterCsvBuilder.Seal(csv),
            VirtualStampReference = string.IsNullOrWhiteSpace(request.VirtualStampReference) ? null : request.VirtualStampReference.Trim(),
            PrintedBy = user.User?.DisplayName ?? string.Empty,
            PrintedAtUtc = time.GetUtcNow().UtcDateTime,
        };

        await unitOfWork.Repository<VatRegisterPrintRun>().AddAsync(run, cancellationToken).ConfigureAwait(false);
        foreach (var entry in printable)
        {
            entry.IsFinalized = true;
            entry.PrintRunId = run.Id;
            registry.Update(entry);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _ = await audit.AppendAsync(
            new AuditEventInput(
                nameof(VatRegisterPrintRun),
                run.Id.ToString(),
                "definitive_print",
                new { run.Kind, run.Section, run.Year, run.FirstProtocol, run.LastProtocol, run.FirstPage, run.LastPage, run.ContentSha256, run.VirtualStampReference }),
            cancellationToken).ConfigureAwait(false);

        return new PrintVatRegisterResult(
            run,
            new GeneratedFile(
                string.Create(CultureInfo.InvariantCulture, $"vat-register-{kind}-{section}-{year}-p{run.FirstPage}-{run.LastPage}.pdf"),
                pdf,
                string.Format(CultureInfo.CurrentCulture, localization["VatCompliance_Print_Summary"], run.EntryCount, run.FirstPage, run.LastPage, run.ContentSha256)));
    }
}
