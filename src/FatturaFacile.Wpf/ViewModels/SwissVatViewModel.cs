// <copyright file="SwissVatViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.SwissVat;
using FatturaFacile.Application.SwissVat;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.SwissVat;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Pure MVVM ViewModel for Swiss VAT (MWST / IVA / TVA) Mod 024:
/// Effective and Saldosteuersatz calculations, Bezugsteuer detection, LIVA Art. 30-33 adjustments,
/// AFC box reconciliation with quadratura validation, and SuisseTax eCH-0217 XML export.
/// </summary>
internal sealed partial class SwissVatViewModel : ObservableObject
{
    private const string NumberFormat = "N2";

    private readonly ISender _sender;
    private readonly ISwissVatApiClient _apiClient;
    private readonly ILocalizationService _localization;
    private readonly IFileDialogService _fileDialog;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _uidMwst = "CHE-116.281.710 MWST";

    [ObservableProperty]
    private string _companyName = "Helvetia AG";

    [ObservableProperty]
    private int _selectedMethodIndex;

    [ObservableProperty]
    private int _selectedFrequencyIndex;

    [ObservableProperty]
    private int _year = 2026;

    [ObservableProperty]
    private int _periodNumber = 1;

    [ObservableProperty]
    private decimal _sssPrimaryRate = 6.20m;

    [ObservableProperty]
    private decimal _sssSecondaryRate;

    [ObservableProperty]
    private decimal _sampleStandardSalesNet = 10000m;

    [ObservableProperty]
    private decimal _sampleReducedSalesNet = 2000m;

    [ObservableProperty]
    private decimal _sampleAccommodationSalesNet;

    [ObservableProperty]
    private decimal _sampleExportNet = 1500m;

    [ObservableProperty]
    private decimal _sampleExemptNoCreditNet;

    [ObservableProperty]
    private decimal _sampleInputVat = 162m;

    [ObservableProperty]
    private string _summaryTurnoverText = "0.00";

    [ObservableProperty]
    private string _summaryOutputVatText = "0.00";

    [ObservableProperty]
    private string _summaryInputVatText = "0.00";

    [ObservableProperty]
    private string _summaryBalanceText = "0.00";

    [ObservableProperty]
    private string _quadraturaBannerText = string.Empty;

    [ObservableProperty]
    private bool _canExportXml;

    [ObservableProperty]
    private string _foreignSupplierName = "CloudTech GmbH";

    [ObservableProperty]
    private string _foreignSupplierCountry = "DE";

    [ObservableProperty]
    private string _foreignInvoiceNumber = "EXT-2026-01";

    [ObservableProperty]
    private decimal _foreignNetChf = 1000m;

    [ObservableProperty]
    private string _adjustmentDescription = string.Empty;

    [ObservableProperty]
    private decimal _adjustmentOriginalCost = 10000m;

    [ObservableProperty]
    private int _adjustmentYearsElapsed = 1;

    [ObservableProperty]
    private int _selectedAdjustmentTypeIndex;

    [ObservableProperty]
    private bool _adjustmentIsRealEstate;

    private SwissVatCalculationResult? _lastResult;
    private readonly List<BezugsteuerEntryDto> _bezugsteuerEntries = [];
    private readonly List<InputTaxAdjustmentDto> _adjustments = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SwissVatViewModel"/> class.
    /// </summary>
    /// <param name="sender">MediatR command/query sender.</param>
    /// <param name="apiClient">Swiss VAT API client.</param>
    /// <param name="localization">Localization service.</param>
    /// <param name="fileDialog">Native file save dialog service.</param>
    public SwissVatViewModel(
        ISender sender,
        ISwissVatApiClient apiClient,
        ILocalizationService localization,
        IFileDialogService fileDialog)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(apiClient);
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(fileDialog);

        _sender = sender;
        _apiClient = apiClient;
        _localization = localization;
        _fileDialog = fileDialog;
    }

    /// <summary>
    /// Gets the AFC box schedule rows.
    /// </summary>
    public ObservableCollection<AfcBoxRow> BoxRows { get; } = [];

    /// <summary>
    /// Gets the quadratura check rows.
    /// </summary>
    public ObservableCollection<QuadraturaCheckRow> QuadraturaRows { get; } = [];

    /// <summary>
    /// Gets the Bezugsteuer entries.
    /// </summary>
    public ObservableCollection<BezugsteuerRow> BezugsteuerRows { get; } = [];

    /// <summary>
    /// Gets the LIVA input tax adjustment entries.
    /// </summary>
    public ObservableCollection<InputTaxAdjustmentRow> AdjustmentRows { get; } = [];

    /// <summary>
    /// Gets the periodic return summaries.
    /// </summary>
    public ObservableCollection<SwissVatReturnRow> ReturnRows { get; } = [];

    /// <summary>
    /// Gets the SuisseTax XML export history rows.
    /// </summary>
    public ObservableCollection<SuisseTaxExportRow> ExportRows { get; } = [];

    /// <summary>
    /// Gets the available years.
    /// </summary>
    public IReadOnlyList<int> Years { get; } = [2024, 2025, 2026, 2027];

    /// <summary>
    /// Gets the accounting method options.
    /// </summary>
    public IReadOnlyList<string> MethodLabels =>
    [
        _localization["SwissVat_Method_Effective"],
        _localization["SwissVat_Method_Saldosteuersatz"],
    ];

    /// <summary>
    /// Gets the reporting frequency options.
    /// </summary>
    public IReadOnlyList<string> FrequencyLabels =>
    [
        _localization["SwissVat_Freq_Quarterly"],
        _localization["SwissVat_Freq_Semiannual"],
        _localization["SwissVat_Freq_Monthly"],
    ];

    /// <summary>
    /// Gets the adjustment type options.
    /// </summary>
    public IReadOnlyList<string> AdjustmentTypeLabels =>
    [
        _localization["SwissVat_Adj_Autoconsumo"],
        _localization["SwissVat_Adj_Degrassazione"],
        _localization["SwissVat_Adj_MixedUse"],
        _localization["SwissVat_Adj_Subsidy"],
    ];

    /// <summary>
    /// Loads remote configuration/returns when authenticated or initializes local workspace.
    /// </summary>
    /// <returns>A task representing the asynchronous load operation.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            if (_apiClient.IsAuthenticated)
            {
                _ = await _apiClient.GetReturnsAsync(Year, CancellationToken.None).ConfigureAwait(true);
            }

            await CalculateAsync().ConfigureAwait(true);
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Calculates the Swiss VAT return and updates AFC boxes and quadratura checks.
    /// </summary>
    /// <returns>A task representing the asynchronous calculation.</returns>
    [RelayCommand]
    public async Task CalculateAsync()
    {
        var method = SelectedMethodIndex == 1 ? SwissVatMethod.Saldosteuersatz : SwissVatMethod.Effective;
        var freq = SelectedFrequencyIndex switch
        {
            1 => SwissVatFrequency.Semiannual,
            2 => SwissVatFrequency.Monthly,
            _ => SwissVatFrequency.Quarterly,
        };

        var (from, to, label) = ResolvePeriodDates(Year, freq, PeriodNumber);
        var epoch = SwissVatRateSchedule.ForDate(to);

        List<SwissVatSaleLineInput> sales = [];
        if (SampleStandardSalesNet > 0m)
        {
            sales.Add(new SwissVatSaleLineInput(SampleStandardSalesNet, epoch.StandardRate, AfcBox: epoch.StandardBox, SssRate: SssPrimaryRate));
        }

        if (SampleReducedSalesNet > 0m)
        {
            sales.Add(new SwissVatSaleLineInput(SampleReducedSalesNet, epoch.ReducedRate, AfcBox: epoch.ReducedBox, SssRate: SssSecondaryRate > 0m ? SssSecondaryRate : SssPrimaryRate));
        }

        if (SampleAccommodationSalesNet > 0m)
        {
            sales.Add(new SwissVatSaleLineInput(SampleAccommodationSalesNet, epoch.AccommodationRate, AfcBox: epoch.AccommodationBox, SssRate: SssPrimaryRate));
        }

        if (SampleExportNet > 0m)
        {
            sales.Add(new SwissVatSaleLineInput(SampleExportNet, 0m, ExemptionType: SwissVatExemptionType.ExemptWithDeduction, AfcBox: "220"));
        }

        if (SampleExemptNoCreditNet > 0m)
        {
            sales.Add(new SwissVatSaleLineInput(SampleExemptNoCreditNet, 0m, ExemptionType: SwissVatExemptionType.ExemptWithoutDeduction, AfcBox: "230"));
        }

        List<SwissVatPurchaseLineInput> purchases = SampleInputVat > 0m
            ? [new SwissVatPurchaseLineInput(0m, SampleInputVat, "400", true)]
            : [];

        var box410 = _adjustments.Where(static a => string.Equals(a.AfcBox, "410", StringComparison.Ordinal)).Sum(static a => a.AdjustmentAmountChf);
        var box415 = _adjustments.Where(static a => string.Equals(a.AfcBox, "415", StringComparison.Ordinal)).Sum(static a => a.AdjustmentAmountChf);
        var box420 = _adjustments.Where(static a => string.Equals(a.AfcBox, "420", StringComparison.Ordinal)).Sum(static a => a.AdjustmentAmountChf);

        var input = new SwissVatCalculationInput(
            PeriodFrom: from,
            PeriodTo: to,
            Method: method,
            ConsiderationBasis: SwissVatConsiderationBasis.Agreed,
            SalesLines: sales,
            PurchaseLines: purchases,
            BezugsteuerEntries: _bezugsteuerEntries,
            AdjustmentsBox410: box410,
            AdjustmentsBox415: box415,
            AdjustmentsBox420: box420,
            SssRatePrimary: SssPrimaryRate,
            SssRateSecondary: SssSecondaryRate > 0m ? SssSecondaryRate : null,
            UseCommercial005Rounding: true);

        var pendingCount = _bezugsteuerEntries.Count(static b => b.Status == BezugsteuerStatus.Detected);
        var outcome = await _sender.Send(new CalculateSwissVatReturnCommand(input, UidMwst, pendingCount), CancellationToken.None).ConfigureAwait(true);

        _lastResult = outcome.Calculation;
        CanExportXml = outcome.Quadratura.Passed && SwissVatRateSchedule.IsValidUidMwst(UidMwst);

        SummaryTurnoverText = outcome.Calculation.Box200TotalTurnover.ToString(NumberFormat, CultureInfo.CurrentCulture);
        SummaryOutputVatText = outcome.Calculation.Box399TotalOutputTax.ToString(NumberFormat, CultureInfo.CurrentCulture);
        SummaryInputVatText = outcome.Calculation.Box479TotalInputTax.ToString(NumberFormat, CultureInfo.CurrentCulture);
        var net = outcome.Calculation.Box500PayableAmount > 0m
            ? outcome.Calculation.Box500PayableAmount
            : -outcome.Calculation.Box510CreditAmount;
        SummaryBalanceText = net.ToString(NumberFormat, CultureInfo.CurrentCulture);

        QuadraturaBannerText = outcome.Quadratura.Passed
            ? _localization["SwissVat_Quad_Passed"]
            : _localization["SwissVat_Quad_Failed"];

        BoxRows.Clear();
        foreach (var b in outcome.Calculation.Boxes)
        {
            BoxRows.Add(new AfcBoxRow(
                b.Code,
                b.Section,
                b.BaseAmount?.ToString(NumberFormat, CultureInfo.CurrentCulture) ?? "-",
                b.RatePercent?.ToString("0.00", CultureInfo.InvariantCulture) ?? "-",
                b.Amount.ToString(NumberFormat, CultureInfo.CurrentCulture)));
        }

        QuadraturaRows.Clear();
        foreach (var q in outcome.Quadratura.Checks)
        {
            QuadraturaRows.Add(new QuadraturaCheckRow(
                q.Code,
                q.Severity,
                q.Passed ? "PASS" : "FAIL",
                q.Expected,
                q.Actual,
                q.Difference,
                _localization[q.MessageKey]));
        }

        ReturnRows.Clear();
        ReturnRows.Add(new SwissVatReturnRow(
            label,
            method == SwissVatMethod.Saldosteuersatz ? _localization["SwissVat_Method_Saldosteuersatz"] : _localization["SwissVat_Method_Effective"],
            SummaryTurnoverText,
            SummaryOutputVatText,
            SummaryInputVatText,
            SummaryBalanceText,
            outcome.Quadratura.Passed ? "OK" : "FAIL"));

        StatusMessage = _localization["SwissVat_Status_Calculated"];
    }

    /// <summary>
    /// Detects and registers a cross-border foreign service subject to Bezugsteuer (Box 382/383).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task DetectForeignServiceAsync()
    {
        var method = SelectedMethodIndex == 1 ? SwissVatMethod.Saldosteuersatz : SwissVatMethod.Effective;
        var entry = await _sender.Send(
            new DetectBezugsteuerCommand(
                ForeignSupplierName,
                ForeignSupplierCountry,
                ForeignInvoiceNumber,
                new DateOnly(Year, Math.Clamp(PeriodNumber, 1, 12), 15),
                "Cloud SaaS / Foreign Consulting",
                ForeignNetChf,
                0m,
                method),
            CancellationToken.None).ConfigureAwait(true);

        if (entry is null)
        {
            StatusMessage = _localization[SwissVatErrors.BezugsteuerDomesticForbidden];
            return;
        }

        var confirmed = entry with { Status = BezugsteuerStatus.Confirmed };
        _bezugsteuerEntries.Add(confirmed);
        BezugsteuerRows.Add(new BezugsteuerRow(
            confirmed.Id,
            confirmed.InvoiceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            confirmed.SupplierName,
            confirmed.SupplierCountry,
            confirmed.ServiceCategory.ToString(),
            confirmed.NetAmountChf.ToString(NumberFormat, CultureInfo.CurrentCulture),
            confirmed.TaxAmountChf.ToString(NumberFormat, CultureInfo.CurrentCulture),
            confirmed.AfcBox,
            confirmed.Status.ToString()));

        await CalculateAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Computes and records a LIVA Art. 30-33 input tax adjustment.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    public async Task AddAdjustmentAsync()
    {
        var type = SelectedAdjustmentTypeIndex switch
        {
            1 => InputTaxAdjustmentType.Degrassazione,
            2 => InputTaxAdjustmentType.MixedUseReduction,
            3 => InputTaxAdjustmentType.SubsidyReduction,
            _ => InputTaxAdjustmentType.Autoconsumo,
        };

        var desc = string.IsNullOrWhiteSpace(AdjustmentDescription) ? type.ToString() : AdjustmentDescription;
        var dto = await _sender.Send(
            new CalculateInputTaxAdjustmentCommand(
                type,
                new DateOnly(Year, 3, 31),
                desc,
                AdjustmentOriginalCost,
                8.10m,
                AdjustmentYearsElapsed,
                AdjustmentIsRealEstate,
                100m),
            CancellationToken.None).ConfigureAwait(true);

        _adjustments.Add(dto);
        AdjustmentRows.Add(new InputTaxAdjustmentRow(
            dto.Id,
            dto.EffectiveDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            dto.AdjustmentType.ToString(),
            dto.AfcBox,
            dto.LegalBasis,
            dto.Description,
            dto.AdjustmentAmountChf.ToString(NumberFormat, CultureInfo.CurrentCulture),
            dto.Status));

        await CalculateAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Validates against the eCH-0217 XSD schema and exports SuisseTax XML to disk.
    /// </summary>
    /// <returns>A task representing the asynchronous export.</returns>
    [RelayCommand]
    public async Task ExportSuisseTaxXmlAsync()
    {
        if (_lastResult is null)
        {
            await CalculateAsync().ConfigureAwait(true);
        }

        if (_lastResult is null)
        {
            return;
        }

        var outcome = await _sender.Send(
            new ValidateSuisseTaxXmlCommand(_lastResult, UidMwst, CompanyName, 0),
            CancellationToken.None).ConfigureAwait(true);

        if (!outcome.Validation.Valid)
        {
            StatusMessage = string.Join(" | ", outcome.Validation.Errors);
            return;
        }

        var defaultFileName = $"suissetax-ech0217-{Year}-P{PeriodNumber}.xml";
        var path = _fileDialog.ShowSaveFile(
            _localization["SwissVat_ExportXml"],
            "XML Files (*.xml)|*.xml",
            defaultFileName);

        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        await File.WriteAllTextAsync(path, outcome.XmlPayload, Encoding.UTF8, CancellationToken.None).ConfigureAwait(true);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(outcome.XmlPayload)));

        ExportRows.Insert(0, new SuisseTaxExportRow(
            Path.GetFileName(path),
            hash,
            outcome.Validation.XsdValid ? "XSD OK" : "INVALID",
            DateTime.UtcNow.ToString("g", CultureInfo.CurrentCulture)));

        StatusMessage = _localization["SwissVat_Status_XmlExported"];
    }

    private static (DateOnly From, DateOnly To, string Label) ResolvePeriodDates(int year, SwissVatFrequency freq, int period)
    {
        return freq switch
        {
            SwissVatFrequency.Semiannual => (
                new DateOnly(year, period == 2 ? 7 : 1, 1),
                new DateOnly(year, period == 2 ? 12 : 6, period == 2 ? 31 : 30),
                $"{year}-S{Math.Clamp(period, 1, 2)}"),
            SwissVatFrequency.Monthly => (
                new DateOnly(year, Math.Clamp(period, 1, 12), 1),
                new DateOnly(year, Math.Clamp(period, 1, 12), DateTime.DaysInMonth(year, Math.Clamp(period, 1, 12))),
                $"{year}-{Math.Clamp(period, 1, 12):00}"),
            _ => ResolveQuarter(year, Math.Clamp(period, 1, 4)),
        };
    }

    private static (DateOnly From, DateOnly To, string Label) ResolveQuarter(int year, int quarter)
    {
        var startMonth = ((quarter - 1) * 3) + 1;
        var from = new DateOnly(year, startMonth, 1);
        var to = from.AddMonths(3).AddDays(-1);
        return (from, to, $"{year}-Q{quarter}");
    }
}
