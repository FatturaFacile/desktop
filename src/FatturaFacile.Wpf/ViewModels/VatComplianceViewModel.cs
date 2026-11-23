// <copyright file="VatComplianceViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Contacts.Queries;
using FatturaFacile.Application.Features.VatCompliance;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.VatCompliance;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The Italian VAT compliance hub (Mod 029): VAT registers with definitive print, declarations of intent and plafond, the LIPE
/// communication, the Erario c/IVA and annual VAT reconciliations, and the Certificazioni Uniche with the Modello F24 payments.
/// </summary>
internal sealed partial class VatComplianceViewModel : ObservableObject
{
    private const string NumberFormat = "N2";
    private const string NoValue = "-";
    private const string DateTimeFormat = "g";
    private const string KeyPrefix = "VatCompliance_";

    private static readonly VatRegisterKind[] Kinds = [.. Enum.GetValues<VatRegisterKind>()];
    private static readonly VatRegisterSection[] Sections = [.. Enum.GetValues<VatRegisterSection>()];

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly IFileDialogService _fileDialog;

    [ObservableProperty]
    private int _year = DateTime.Today.Year;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private int _selectedKindIndex;

    [ObservableProperty]
    private int _selectedSectionIndex;

    [ObservableProperty]
    private DateTime _registerFrom = new(DateTime.Today.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [ObservableProperty]
    private DateTime _registerTo = new(DateTime.Today.Year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

    [ObservableProperty]
    private string _proRataText = string.Empty;

    [ObservableProperty]
    private string _stampReference = string.Empty;

    [ObservableProperty]
    private string _registerTotalsText = string.Empty;

    [ObservableProperty]
    private string _gapsText = string.Empty;

    [ObservableProperty]
    private bool _hasGaps;

    [ObservableProperty]
    private DateTime _receiptDate = DateTime.Today;

    [ObservableProperty]
    private string _receiptGrossText = string.Empty;

    [ObservableProperty]
    private string _goodsDocument = string.Empty;

    [ObservableProperty]
    private string _goodsPartner = string.Empty;

    [ObservableProperty]
    private string _goodsDescription = string.Empty;

    [ObservableProperty]
    private string _goodsValueText = string.Empty;

    [ObservableProperty]
    private IntentLetterRow? _selectedLetter;

    [ObservableProperty]
    private ContactOption? _letterContact;

    [ObservableProperty]
    private string _letterProtocol = string.Empty;

    [ObservableProperty]
    private DateTime _letterReceiptDate = DateTime.Today;

    [ObservableProperty]
    private string _letterPlafondText = string.Empty;

    [ObservableProperty]
    private DateTime _letterValidFrom = new(DateTime.Today.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    [ObservableProperty]
    private DateTime _letterValidTo = new(DateTime.Today.Year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

    [ObservableProperty]
    private string _letterNotes = string.Empty;

    [ObservableProperty]
    private ContactOption? _checkContact;

    [ObservableProperty]
    private string _checkAmountText = string.Empty;

    [ObservableProperty]
    private DateTime _checkDate = DateTime.Today;

    [ObservableProperty]
    private string _checkResultText = string.Empty;

    [ObservableProperty]
    private LipeRow? _selectedLipe;

    [ObservableProperty]
    private string _receiptProtocolText = string.Empty;

    [ObservableProperty]
    private string _receiptNoteText = string.Empty;

    [ObservableProperty]
    private string _erarioSummaryText = string.Empty;

    [ObservableProperty]
    private string _annualSummaryText = string.Empty;

    [ObservableProperty]
    private string _annualFindingsText = string.Empty;

    [ObservableProperty]
    private string _cuSummaryText = string.Empty;

    [ObservableProperty]
    private F24Row? _selectedPayment;

    [ObservableProperty]
    private DateTime _paymentDate = DateTime.Today;

    [ObservableProperty]
    private string _paymentReference = string.Empty;

    private Guid? _editingLetterId;

    /// <summary>
    /// Initializes a new instance of the <see cref="VatComplianceViewModel"/> class.
    /// </summary>
    /// <param name="sender">The mediator.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="fileDialog">The file dialogs.</param>
    public VatComplianceViewModel(ISender sender, ILocalizationService localization, IFileDialogService fileDialog)
    {
        _sender = sender;
        _localization = localization;
        _fileDialog = fileDialog;
        LastSyncedText = _localization[KeyPrefix + "NeverSynced"];
        KindNames = [.. Kinds.Select(k => _localization[KeyPrefix + "Kind_" + k])];
        SectionNames = [_localization[KeyPrefix + "Section_All"], .. Sections.Select(s => _localization[KeyPrefix + "Section_" + s])];
        Years = [.. Enumerable.Range(DateTime.Today.Year - 5, 7)];
    }

    /// <summary>
    /// Gets the localized names of the registers.
    /// </summary>
    public IReadOnlyList<string> KindNames { get; }

    /// <summary>
    /// Gets the localized names of the sezionali, the first one meaning all of them.
    /// </summary>
    public IReadOnlyList<string> SectionNames { get; }

    /// <summary>
    /// Gets the years that can be worked on.
    /// </summary>
    public IReadOnlyList<int> Years { get; }

    /// <summary>
    /// Gets the customers and suppliers a letter can be issued by or to.
    /// </summary>
    internal ObservableCollection<ContactOption> Contacts { get; } = [];

    /// <summary>
    /// Gets the lines of the register shown.
    /// </summary>
    internal ObservableCollection<VatRegisterRow> RegisterLines { get; } = [];

    /// <summary>
    /// Gets the totals per rate of the register shown.
    /// </summary>
    internal ObservableCollection<VatRateRow> RateTotals { get; } = [];

    /// <summary>
    /// Gets the declarations of intent.
    /// </summary>
    internal ObservableCollection<IntentLetterRow> Letters { get; } = [];

    /// <summary>
    /// Gets the liquidations with their LIPE communications.
    /// </summary>
    internal ObservableCollection<LipeRow> LipeRows { get; } = [];

    /// <summary>
    /// Gets the periods of the Erario c/IVA reconciliation.
    /// </summary>
    internal ObservableCollection<ErarioRow> ErarioRows { get; } = [];

    /// <summary>
    /// Gets the periods of the annual reconciliation.
    /// </summary>
    internal ObservableCollection<AnnualRow> AnnualRows { get; } = [];

    /// <summary>
    /// Gets the Certificazioni Uniche.
    /// </summary>
    internal ObservableCollection<CertificateRow> Certificates { get; } = [];

    /// <summary>
    /// Gets the Modello F24 payments of the withholdings.
    /// </summary>
    internal ObservableCollection<F24Row> Payments { get; } = [];

    private VatRegisterKind CurrentKind => Kinds[Math.Clamp(SelectedKindIndex, 0, Kinds.Length - 1)];

    private VatRegisterSection? CurrentSection => SelectedSectionIndex <= 0 ? null : Sections[Math.Min(SelectedSectionIndex - 1, Sections.Length - 1)];

    /// <summary>
    /// Loads every section of the year from the local database.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var contacts = await _sender.Send(new GetContactsQuery());
        Contacts.Clear();
        foreach (var contact in contacts.OrderBy(static c => c.CompanyName ?? c.LastName, StringComparer.OrdinalIgnoreCase))
        {
            Contacts.Add(new ContactOption(contact.Id, LabelOf(contact)));
        }

        await ShowAllAsync();
    });

    /// <summary>
    /// Syncs the year with the web-api and shows the result; offline, the local data stays on screen.
    /// </summary>
    /// <returns>A task representing the sync.</returns>
    [RelayCommand]
    public Task SyncAsync() => RunAsync(async () =>
    {
        var result = await _sender.Send(new SyncVatComplianceCommand(Year));
        StatusMessage = result.Status == VatComplianceSyncStatus.Synced
            ? string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "Sync_Synced"], result.Pushed, result.Pulled, result.Conflicts, result.ProtocolsMatched)
            : _localization[KeyPrefix + "Sync_" + result.Status];

        if (result.SyncedAtUtc is { } synced)
        {
            LastSyncedText = string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "LastSynced"], synced.ToLocalTime().ToString(DateTimeFormat, CultureInfo.CurrentCulture));
        }

        await ShowAllAsync();
    });

    /// <summary>
    /// Shows the register chosen over the period chosen.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadRegisterAsync() => RunAsync(ShowRegisterAsync);

    /// <summary>
    /// Registers the documents of the period in the sales and purchases registers.
    /// </summary>
    /// <returns>A task representing the registration.</returns>
    [RelayCommand]
    public Task RegisterDocumentsAsync() => RunAsync(async () =>
    {
        if (!TryParseOptional(ProRataText, out var proRata))
        {
            StatusMessage = _localization[VatComplianceErrors.Percent];

            return;
        }

        var result = await _sender.Send(new RegisterVatDocumentsCommand(RegisterFrom, RegisterTo, proRata));
        StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "Registered"], result.SalesLines, result.PurchaseLines, result.ProRataPercent.ToString("0.##", CultureInfo.CurrentCulture));
        await ShowRegisterAsync();
    });

    /// <summary>
    /// Exports the register of the period as a PDF preview.
    /// </summary>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public Task ExportRegisterPdfAsync() => RunAsync(() => ExportRegisterAsync(pdf: true));

    /// <summary>
    /// Exports the register of the period as a CSV file.
    /// </summary>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public Task ExportRegisterCsvAsync() => RunAsync(() => ExportRegisterAsync(pdf: false));

    /// <summary>
    /// Prints the register definitively: the lines are locked and the pages numbered.
    /// </summary>
    /// <returns>A task representing the print.</returns>
    [RelayCommand]
    public Task PrintRegisterAsync() => RunAsync(async () =>
    {
        var result = await _sender.Send(new PrintVatRegisterCommand(CurrentKind, CurrentSection ?? VatRegisterSection.Ordinary, RegisterFrom.Year, RegisterFrom, RegisterTo, StampReference));
        await SaveAsync(result.Pdf, KeyPrefix + "FilterPdf");
        await ShowRegisterAsync();
    });

    /// <summary>
    /// Records the takings of a day in the register of corrispettivi, ventilated over the rates.
    /// </summary>
    /// <returns>A task representing the registration.</returns>
    [RelayCommand]
    public Task RecordReceiptsAsync() => RunAsync(async () =>
    {
        if (!TryParse(ReceiptGrossText, out var gross))
        {
            StatusMessage = _localization[VatComplianceErrors.Amount];

            return;
        }

        var lines = await _sender.Send(new RecordReceiptsCommand(ReceiptDate, gross, []));
        StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "ReceiptsRecorded"], lines);
        ReceiptGrossText = string.Empty;
        SelectedKindIndex = Array.IndexOf(Kinds, VatRegisterKind.Receipts);
        await ShowRegisterAsync();
    });

    /// <summary>
    /// Records goods held for a third party in the register of merci in conto terzi.
    /// </summary>
    /// <returns>A task representing the registration.</returns>
    [RelayCommand]
    public Task RecordGoodsAsync() => RunAsync(async () =>
    {
        if (!TryParse(GoodsValueText, out var value))
        {
            StatusMessage = _localization[VatComplianceErrors.Amount];

            return;
        }

        _ = await _sender.Send(new RecordThirdPartyGoodsCommand(ReceiptDate, GoodsDocument, GoodsPartner, GoodsDescription, value));
        StatusMessage = _localization[KeyPrefix + "GoodsRecorded"];
        GoodsValueText = string.Empty;
        GoodsDescription = string.Empty;
        SelectedKindIndex = Array.IndexOf(Kinds, VatRegisterKind.ThirdPartyGoods);
        await ShowRegisterAsync();
    });

    /// <summary>
    /// Clears the letter form to enter a new declaration of intent.
    /// </summary>
    [RelayCommand]
    public void NewLetter()
    {
        _editingLetterId = null;
        SelectedLetter = null;
        LetterContact = null;
        LetterProtocol = string.Empty;
        LetterPlafondText = string.Empty;
        LetterNotes = string.Empty;
        LetterReceiptDate = DateTime.Today;
        LetterValidFrom = new DateTime(Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        LetterValidTo = new DateTime(Year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Saves the declaration of intent of the form offline.
    /// </summary>
    /// <returns>A task representing the save.</returns>
    [RelayCommand]
    public Task SaveLetterAsync() => RunAsync(async () =>
    {
        if (LetterContact is null || !TryParse(LetterPlafondText, out var plafond))
        {
            StatusMessage = _localization[VatComplianceErrors.Required];

            return;
        }

        IntentLetterInput input = new(_editingLetterId, LetterContact.Id, IntentLetterDirection.Received, LetterProtocol, LetterReceiptDate, Year, plafond, LetterValidFrom, LetterValidTo, LetterNotes);
        _ = await _sender.Send(new SaveIntentLetterCommand(input));
        StatusMessage = _localization[KeyPrefix + "LetterSaved"];
        NewLetter();
        await ShowLettersAsync();
    });

    /// <summary>
    /// Deletes the selected declaration of intent when no invoice used it.
    /// </summary>
    /// <returns>A task representing the deletion.</returns>
    [RelayCommand]
    public Task DeleteLetterAsync() => RunAsync(async () =>
    {
        if (SelectedLetter is not { } letter)
        {
            return;
        }

        _ = await _sender.Send(new DeleteIntentLetterCommand(letter.Id));
        StatusMessage = _localization[KeyPrefix + "LetterDeleted"];
        NewLetter();
        await ShowLettersAsync();
    });

    /// <summary>
    /// Checks whether an invoice without VAT of the given amount may be issued to the customer.
    /// </summary>
    /// <returns>A task representing the check.</returns>
    [RelayCommand]
    public Task CheckPlafondAsync() => RunAsync(async () =>
    {
        if (CheckContact is null || !TryParse(CheckAmountText, out var amount))
        {
            StatusMessage = _localization[VatComplianceErrors.Required];

            return;
        }

        var result = await _sender.Send(new CheckIntentPlafondQuery(CheckContact.Id, CheckDate, amount));
        var residual = result.Residual.ToString(NumberFormat, CultureInfo.CurrentCulture);
        var allowedKey = result.NearExhaustion ? "Check_NearExhaustion" : "Check_Allowed";
        CheckResultText = result.Allowed
            ? string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + allowedKey], result.ProtocolNumber, residual)
            : string.Format(CultureInfo.CurrentCulture, _localization[result.BlockCode ?? VatComplianceErrors.BlockNoValidLetter], result.ProtocolNumber, residual);
    });

    /// <summary>
    /// Generates and validates the LIPE communication of the selected liquidation.
    /// </summary>
    /// <returns>A task representing the preparation.</returns>
    [RelayCommand]
    public Task PrepareLipeAsync() => RunAsync(async () =>
    {
        if (SelectedLipe is not { } row)
        {
            return;
        }

        var submission = await _sender.Send(new PrepareLipeSubmissionCommand(row.ReturnId));
        StatusMessage = _localization[KeyPrefix + (submission.IsXsdValid ? "Lipe_Valid" : "Lipe_Invalid")];
        await ShowLipeAsync();
    });

    /// <summary>
    /// Saves the XML of the selected communication.
    /// </summary>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public Task ExportLipeAsync() => RunAsync(async () =>
    {
        if (SelectedLipe?.SubmissionId is not { } id)
        {
            return;
        }

        await SaveAsync(await _sender.Send(new ExportLipeSubmissionQuery(id)), KeyPrefix + "FilterXml");
    });

    /// <summary>
    /// Attaches the signed copy of the selected communication.
    /// </summary>
    /// <returns>A task representing the attachment.</returns>
    [RelayCommand]
    public Task AttachSignedAsync() => RunAsync(async () =>
    {
        if (SelectedLipe?.SubmissionId is not { } id)
        {
            return;
        }

        var path = _fileDialog.ShowOpenFile(_localization[KeyPrefix + "Lipe_OpenSigned"], _localization[KeyPrefix + "FilterSigned"]);
        if (path is null)
        {
            return;
        }

        _ = await _sender.Send(new AttachSignedLipeCommand(id, Path.GetFileName(path), await File.ReadAllBytesAsync(path)));
        StatusMessage = _localization[KeyPrefix + "Lipe_SignatureOk"];
        await ShowLipeAsync();
    });

    /// <summary>
    /// Records that the selected communication was transmitted.
    /// </summary>
    /// <returns>A task representing the update.</returns>
    [RelayCommand]
    public Task MarkTransmittedAsync() => UpdateTransmissionAsync(LipeTransmissionStatus.Transmitted);

    /// <summary>
    /// Records that the tax authority accepted the selected communication.
    /// </summary>
    /// <returns>A task representing the update.</returns>
    [RelayCommand]
    public Task MarkAcceptedAsync() => UpdateTransmissionAsync(LipeTransmissionStatus.Accepted);

    /// <summary>
    /// Records that the tax authority rejected the selected communication.
    /// </summary>
    /// <returns>A task representing the update.</returns>
    [RelayCommand]
    public Task MarkRejectedAsync() => UpdateTransmissionAsync(LipeTransmissionStatus.Rejected);

    /// <summary>
    /// Builds the Certificazioni Uniche of the year from the withholdings.
    /// </summary>
    /// <returns>A task representing the build.</returns>
    [RelayCommand]
    public Task BuildCertificatesAsync() => RunAsync(async () =>
    {
        var count = await _sender.Send(new BuildCertificazioniUnicheCommand(Year));
        StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "CuBuilt"], count);
        await ShowCuAsync();
    });

    /// <summary>
    /// Exports the Certificazioni Uniche as a PDF.
    /// </summary>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public Task ExportCuPdfAsync() => RunAsync(async () =>
    {
        await SaveAsync(await _sender.Send(new ExportCuPdfQuery(Year)), KeyPrefix + "FilterPdf");
        await ShowCuAsync();
    });

    /// <summary>
    /// Exports the working file of the Certificazioni Uniche for the telematic channel.
    /// </summary>
    /// <returns>A task representing the export.</returns>
    [RelayCommand]
    public Task ExportCuTelematicAsync() => RunAsync(async () =>
    {
        await SaveAsync(await _sender.Send(new ExportCuTelematicQuery(Year)), KeyPrefix + "FilterText");
        await ShowCuAsync();
    });

    /// <summary>
    /// Records the Modello F24 payment selected.
    /// </summary>
    /// <returns>A task representing the update.</returns>
    [RelayCommand]
    public Task RecordPaymentAsync() => RunAsync(async () =>
    {
        if (SelectedPayment is not { } payment)
        {
            return;
        }

        _ = await _sender.Send(new RecordWithholdingPaymentCommand(payment.Id, PaymentDate, PaymentReference));
        StatusMessage = _localization[KeyPrefix + "PaymentRecorded"];
        PaymentReference = string.Empty;
        await ShowCuAsync();
    });

    private static string LabelOf(ContactDto contact) =>
        string.IsNullOrWhiteSpace(contact.CompanyName) ? $"{contact.FirstName} {contact.LastName}".Trim() : contact.CompanyName;

    private static string Money(decimal value) => value.ToString(NumberFormat, CultureInfo.CurrentCulture);

    private static string Day(DateTime value) => value.ToString("d", CultureInfo.CurrentCulture);

    private static bool TryParse(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value);

    private static bool TryParseOptional(string text, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (!TryParse(text, out var parsed) || parsed is < 0m or > 100m)
        {
            return false;
        }

        value = parsed;

        return true;
    }

    private static bool IsHandled(Exception ex) => IsDomainFailure(ex) || ex is UnauthorizedAccessException or IOException;

    private static bool IsDomainFailure(Exception ex) => ex is InvalidOperationException or KeyNotFoundException or ValidationException;

    partial void OnYearChanged(int value)
    {
        RegisterFrom = new DateTime(value, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        RegisterTo = new DateTime(value, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
        _ = LoadAsync();
    }

    partial void OnSelectedLetterChanged(IntentLetterRow? value)
    {
        if (value is null)
        {
            return;
        }

        _ = LoadLetterIntoFormAsync(value);
    }

    private async Task LoadLetterIntoFormAsync(IntentLetterRow row)
    {
        var letters = await _sender.Send(new GetIntentLettersQuery(Year));
        if (letters.FirstOrDefault(l => l.Letter.Id == row.Id) is not { } view)
        {
            return;
        }

        var letter = view.Letter;
        _editingLetterId = letter.Id;
        LetterContact = Contacts.FirstOrDefault(c => c.Id == letter.ContactId);
        LetterProtocol = letter.ProtocolNumber;
        LetterReceiptDate = letter.ReceiptDate;
        LetterPlafondText = letter.PlafondAmount.ToString(NumberFormat, CultureInfo.CurrentCulture);
        LetterValidFrom = letter.ValidFrom;
        LetterValidTo = letter.ValidTo;
        LetterNotes = letter.Notes ?? string.Empty;
    }

    private string Describe(Exception ex)
    {
        var key = ex is ValidationException validation ? validation.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message : ex.Message;

        return _localization[key];
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            await action();
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            StatusMessage = Describe(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task UpdateTransmissionAsync(LipeTransmissionStatus status) => RunAsync(async () =>
    {
        if (SelectedLipe?.SubmissionId is not { } id)
        {
            return;
        }

        _ = await _sender.Send(new UpdateLipeTransmissionCommand(id, status, ReceiptProtocolText, ReceiptNoteText));
        StatusMessage = _localization[KeyPrefix + "Lipe_Updated"];
        await ShowLipeAsync();
    });

    private async Task SaveAsync(GeneratedFile file, string filterKey)
    {
        var path = _fileDialog.ShowSaveFile(_localization[KeyPrefix + "SaveTitle"], _localization[filterKey], file.FileName);
        if (path is null)
        {
            return;
        }

        await File.WriteAllBytesAsync(path, file.Content);
        StatusMessage = string.IsNullOrEmpty(file.Summary) ? Path.GetFileName(path) : file.Summary;
    }

    private async Task ExportRegisterAsync(bool pdf)
    {
        var file = await _sender.Send(new ExportVatRegisterQuery(CurrentKind, CurrentSection, RegisterFrom, RegisterTo, pdf));
        await SaveAsync(file, KeyPrefix + (pdf ? "FilterPdf" : "FilterCsv"));
    }

    private async Task ShowAllAsync()
    {
        await ShowRegisterAsync();
        await ShowLettersAsync();
        await ShowLipeAsync();
        await ShowErarioAsync();
        await ShowAnnualAsync();
        await ShowCuAsync();
    }

    private async Task ShowRegisterAsync()
    {
        var report = await _sender.Send(new GetVatRegisterQuery(CurrentKind, CurrentSection, RegisterFrom, RegisterTo));

        RegisterLines.Clear();
        foreach (var line in report.Lines)
        {
            RegisterLines.Add(new VatRegisterRow(
                line.Section + "/" + line.ProtocolNumber.ToString(CultureInfo.InvariantCulture),
                Day(line.RegistrationDate),
                line.DocumentNumber,
                line.PartnerName ?? line.Description ?? string.Empty,
                Money(line.TaxableAmount),
                line.VatRate.ToString("0.##", CultureInfo.CurrentCulture) + (line.VatNature is null ? string.Empty : " " + line.VatNature),
                Money(line.VatAmount),
                Money(line.DeductibleVat),
                Money(line.NonDeductibleVat),
                _localization[KeyPrefix + (line.IsFinalized ? "State_Printed" : "State_Open")]));
        }

        RateTotals.Clear();
        foreach (var total in report.RateTotals)
        {
            RateTotals.Add(new VatRateRow(
                total.VatRate.ToString("0.##", CultureInfo.CurrentCulture) + (total.Nature is null ? string.Empty : " " + total.Nature),
                Money(total.Taxable),
                Money(total.Vat),
                Money(total.Deductible),
                Money(total.NonDeductible)));
        }

        RegisterTotalsText = string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "Pdf_Totals"], Money(report.TotalTaxable), Money(report.TotalVat), Money(report.TotalDeductible), Money(report.TotalNonDeductible));
        HasGaps = report.Gaps.Count > 0;
        GapsText = HasGaps ? string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + "Gaps"], string.Join(", ", report.Gaps)) : string.Empty;
    }

    private async Task ShowLettersAsync()
    {
        var letters = await _sender.Send(new GetIntentLettersQuery(Year));

        Letters.Clear();
        foreach (var view in letters)
        {
            var letter = view.Letter;
            Letters.Add(new IntentLetterRow(
                letter.Id,
                letter.ProtocolNumber,
                view.ContactName,
                Day(letter.ReceiptDate),
                Day(letter.ValidFrom) + " - " + Day(letter.ValidTo),
                Money(letter.PlafondAmount),
                Money(view.Consumed),
                Money(view.Residual),
                _localization[KeyPrefix + "SyncState_" + letter.SyncState]));
        }
    }

    private async Task ShowLipeAsync()
    {
        var overview = await _sender.Send(new GetLipeOverviewQuery(Year));

        LipeRows.Clear();
        foreach (var item in overview)
        {
            var vat = item.Return;
            var submission = item.Submission;
            LipeRows.Add(new LipeRow(
                vat.Id,
                submission?.Id,
                Day(vat.PeriodFrom) + " - " + Day(vat.PeriodTo),
                Money(vat.OutputVat),
                Money(vat.InputVat),
                Money(vat.Payable),
                _localization[KeyPrefix + "LipeStatus_" + (submission?.Status ?? LipeTransmissionStatus.Draft)] + (submission is null ? " (" + _localization[KeyPrefix + "Lipe_NotPrepared"] + ")" : string.Empty),
                DescribeValidation(submission)));
        }
    }

    private string DescribeValidation(LipeSubmission? submission) =>
        string.IsNullOrEmpty(submission?.ValidationErrors)
            ? submission?.XmlSha256 ?? string.Empty
            : string.Join("; ", submission.ValidationErrors.Split('\n').Select(Rule));

    private string Rule(string code) =>
        code.StartsWith("xsd:", StringComparison.Ordinal) ? code : _localization[KeyPrefix + "LipeRule_" + code];

    private async Task ShowErarioAsync()
    {
        var erario = await _sender.Send(new GetErarioReconciliationQuery(Year));

        ErarioRows.Clear();
        foreach (var row in erario.Rows)
        {
            ErarioRows.Add(new ErarioRow(
                Day(row.From) + " - " + Day(row.To),
                Money(row.LocalBalance),
                row.ServerPayable is { } server ? Money(server) : NoValue,
                Money(row.LedgerPosted),
                Money(row.Difference),
                _localization[KeyPrefix + "Erario_" + row.State]));
        }

        var summaryKey = erario.IsReconciled ? "Erario_SummaryOk" : "Erario_SummaryGap";
        ErarioSummaryText = erario.AccountFound
            ? string.Format(CultureInfo.CurrentCulture, _localization[KeyPrefix + summaryKey], Money(erario.TotalLocal), Money(erario.TotalLedger), Money(erario.TotalDifference))
            : _localization[KeyPrefix + "Erario_NoAccount"];
    }

    private async Task ShowAnnualAsync()
    {
        var annual = await _sender.Send(new GetAnnualVatReconciliationQuery(Year));

        AnnualRows.Clear();
        foreach (var row in annual.Rows)
        {
            AnnualRows.Add(new AnnualRow(
                Day(row.From) + " - " + Day(row.To),
                Money(row.OutputVat),
                Money(row.InputVat),
                Money(row.PreviousCredit),
                Money(row.Interest),
                Money(row.Payable),
                row.Status));
        }

        AnnualSummaryText = string.Format(
            CultureInfo.CurrentCulture,
            _localization[KeyPrefix + "Annual_Summary"],
            Money(annual.TotalOutputVat),
            Money(annual.TotalInputVat),
            Money(annual.TotalInterest),
            Money(annual.OpeningCredit),
            Money(annual.AnnualBalance),
            Money(annual.SalesTaxable),
            Money(annual.SalesVat),
            Money(annual.PurchasesTaxable),
            Money(annual.PurchasesDeductibleVat));
        AnnualFindingsText = annual.Findings.Count == 0
            ? _localization[KeyPrefix + "Annual_NoFindings"]
            : string.Join(Environment.NewLine, annual.Findings.Select(f => _localization[KeyPrefix + "Finding_" + f]));
    }

    private async Task ShowCuAsync()
    {
        var certificates = await _sender.Send(new GetCertificazioniUnicheQuery(Year));
        var payments = await _sender.Send(new GetWithholdingPaymentsQuery(Year));

        Certificates.Clear();
        foreach (var certificate in certificates)
        {
            Certificates.Add(new CertificateRow(
                certificate.Id,
                certificate.RecipientName,
                certificate.RecipientTaxCode,
                certificate.Reason,
                Money(certificate.GrossPaid),
                Money(certificate.TaxableAmount),
                Money(certificate.WithheldAmount),
                Money(certificate.WithheldPaidAmount),
                _localization[KeyPrefix + "CuStatus_" + certificate.Status]));
        }

        Payments.Clear();
        foreach (var payment in payments)
        {
            Payments.Add(new F24Row(
                payment.Id,
                payment.Month.ToString("00", CultureInfo.InvariantCulture) + "/" + payment.Year.ToString(CultureInfo.InvariantCulture),
                payment.TaxCode,
                Money(payment.Amount),
                Day(payment.DueDate),
                payment.PaidDate is { } paid ? Day(paid) : NoValue,
                payment.PaymentReference ?? string.Empty,
                _localization[KeyPrefix + "SyncState_" + payment.SyncState]));
        }

        CuSummaryText = string.Format(
            CultureInfo.CurrentCulture,
            _localization[KeyPrefix + "Cu_Summary"],
            certificates.Count,
            Money(certificates.Sum(static c => c.WithheldAmount)),
            Money(certificates.Sum(static c => c.WithheldPaidAmount)));
    }
}
