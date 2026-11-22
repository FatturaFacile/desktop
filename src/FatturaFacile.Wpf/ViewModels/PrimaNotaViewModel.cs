// <copyright file="PrimaNotaViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Commands;
using FatturaFacile.Application.Features.Accounting.Queries;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The Prima Nota (journal entry): compose a balanced double-entry in a ledger book, optionally from a template (causale contabile),
/// watch Dare, Avere and Differenza live, post only when balanced, and manage the journal with storno.
/// </summary>
internal sealed partial class PrimaNotaViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ILocalizationService _localization;

    [ObservableProperty]
    private DateTime _postingDate = DateTime.Today;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private LedgerBookChoice _selectedLedger;

    [ObservableProperty]
    private decimal _totalDebit;

    [ObservableProperty]
    private decimal _totalCredit;

    [ObservableProperty]
    private decimal _difference;

    [ObservableProperty]
    private bool _isBalanced;

    [ObservableProperty]
    private string _balanceMessage = string.Empty;

    [ObservableProperty]
    private string _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private TemplateDefinition? _selectedTemplate;

    [ObservableProperty]
    private decimal _templateAmount;

    [ObservableProperty]
    private decimal _templateTaxRate = 22m;

    [ObservableProperty]
    private bool _templateNeedsRate;

    [ObservableProperty]
    private JournalEntryRow? _selectedEntry;

    [ObservableProperty]
    private string _reversalReason = string.Empty;

    [ObservableProperty]
    private DateTime _reversalDate = DateTime.Today;

    private bool _canPost;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaNotaViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="localization">The localization service.</param>
    public PrimaNotaViewModel(ISender sender, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(localization);

        _sender = sender;
        _localization = localization;
        LedgerBooks = LedgerBookChoices.Build(localization);
        _selectedLedger = LedgerBooks[0];
        _statusMessage = localization["Common_Ready"];

        ResetLines();
    }

    /// <summary>
    /// Gets the ledger books of the selector.
    /// </summary>
    public IReadOnlyList<LedgerBookChoice> LedgerBooks { get; }

    /// <summary>
    /// Gets the lines of the entry being composed.
    /// </summary>
    public ObservableCollection<JournalLineRow> Lines { get; } = [];

    /// <summary>
    /// Gets the accounts a manual entry can be posted to.
    /// </summary>
    public ObservableCollection<ChartOfAccountDto> Accounts { get; } = [];

    /// <summary>
    /// Gets the templates (causali contabili).
    /// </summary>
    public ObservableCollection<TemplateDefinition> Templates { get; } = [];

    /// <summary>
    /// Gets the account choices of the selected template.
    /// </summary>
    public ObservableCollection<TemplateAccountInput> TemplateAccounts { get; } = [];

    /// <summary>
    /// Gets the journal.
    /// </summary>
    public ObservableCollection<JournalEntryRow> Entries { get; } = [];

    /// <summary>
    /// Loads the chart of accounts, the templates and the journal.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var accounts = await _sender.Send(new GetChartOfAccountsQuery());
            Accounts.Clear();
            foreach (var account in accounts.Where(static a => a.AllowsManualPosting))
            {
                Accounts.Add(account);
            }

            var templates = await _sender.Send(new GetAccountingTemplatesQuery());
            Templates.Clear();
            foreach (var template in templates)
            {
                Templates.Add(template);
            }

            await RefreshEntriesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = JournalMessages.Describe(ex, _localization);
        }
    }

    /// <summary>
    /// Reloads the journal list.
    /// </summary>
    /// <returns>A task representing the reload.</returns>
    public async Task RefreshEntriesAsync()
    {
        var entries = await _sender.Send(new GetGeneralLedgerEntriesQuery());
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries.Add(new JournalEntryRow(entry, LedgerLabel(entry), StatusOf(entry)));
        }

        SelectedEntry = null;
    }

    [RelayCommand]
    private void AddLine()
    {
        Attach(new JournalLineRow());
        Recalculate();
    }

    [RelayCommand]
    private void RemoveLine(JournalLineRow? line)
    {
        if (line is null || !Lines.Remove(line))
        {
            return;
        }

        line.Changed -= OnLineChanged;
        Recalculate();
    }

    [RelayCommand]
    private void NewEntry()
    {
        Description = string.Empty;
        PostingDate = DateTime.Today;
        SelectedTemplate = null;
        TemplateAmount = 0m;
        ResetLines();
        StatusMessage = _localization["Common_Ready"];
    }

    [RelayCommand]
    private async Task ApplyTemplateAsync()
    {
        if (SelectedTemplate is not { } template)
        {
            return;
        }

        try
        {
            var chosen = TemplateAccounts
                .Where(static i => i.Account is not null)
                .ToDictionary(static i => i.Key, static i => i.Account!.Code ?? i.Account.Id.ToString(), StringComparer.Ordinal);

            var preview = await _sender.Send(new PreviewAccountingTemplateQuery(
                template,
                new TemplateInput(TemplateAmount, template.NeedsRate ? TemplateTaxRate : null, chosen)));

            if (preview.Issues.Count > 0)
            {
                StatusMessage = string.Join(' ', preview.Issues.Select(DescribeIssue));
                return;
            }

            SelectedLedger = LedgerBooks.First(b => b.Book == template.Ledger);
            if (string.IsNullOrWhiteSpace(Description))
            {
                Description = template.Name;
            }

            foreach (var line in Lines)
            {
                line.Changed -= OnLineChanged;
            }

            Lines.Clear();
            foreach (var line in preview.Lines)
            {
                Attach(new JournalLineRow
                {
                    Account = Accounts.FirstOrDefault(a => a.Id == line.AccountId),
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Description = line.Description ?? string.Empty,
                });
            }

            Recalculate();
        }
        catch (Exception ex)
        {
            StatusMessage = JournalMessages.Describe(ex, _localization);
        }
    }

    [RelayCommand]
    private Task SaveDraftAsync() => CreateAsync(post: false);

    [RelayCommand(CanExecute = nameof(CanPost))]
    private Task PostAsync() => CreateAsync(post: true);

    [RelayCommand]
    private async Task PostSelectedAsync()
    {
        if (SelectedEntry is not { CanPost: true } row)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var posted = await _sender.Send(new PostJournalEntryCommand(row.Entry.Id));
            await RefreshEntriesAsync();
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization["Gl_EntryPosted"], posted.EntryNumber);
        });
    }

    [RelayCommand]
    private async Task ReverseSelectedAsync()
    {
        if (SelectedEntry is not { CanReverse: true } row)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var reversal = await _sender.Send(new ReverseJournalEntryCommand(row.Entry.Id, ReversalDate, ReversalReason));
            await RefreshEntriesAsync();
            ReversalReason = string.Empty;
            StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization["Gl_EntryReversed"], reversal.EntryNumber);
        });
    }

    [RelayCommand]
    private Task PullTemplatesAsync() => RunAsync(async () =>
    {
        var count = await _sender.Send(new PullAccountingTemplatesCommand());
        var templates = await _sender.Send(new GetAccountingTemplatesQuery());
        Templates.Clear();
        foreach (var template in templates)
        {
            Templates.Add(template);
        }

        StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization["Gl_TemplatesPulled"], count);
    });

    partial void OnSelectedTemplateChanged(TemplateDefinition? value)
    {
        TemplateAccounts.Clear();
        TemplateNeedsRate = value?.NeedsRate ?? false;

        if (value is null)
        {
            return;
        }

        foreach (var key in value.AccountKeys)
        {
            TemplateAccounts.Add(new TemplateAccountInput(key));
        }
    }

    partial void OnSelectedEntryChanged(JournalEntryRow? value)
    {
        ReverseSelectedCommand.NotifyCanExecuteChanged();
        PostSelectedCommand.NotifyCanExecuteChanged();
    }

    partial void OnDescriptionChanged(string value) => Recalculate();

    private bool CanPost() => _canPost;

    private string LedgerLabel(GeneralLedgerEntryDto entry) => LedgerBooks.First(b => b.Book == entry.LedgerType).Label;

    private string StatusOf(GeneralLedgerEntryDto entry)
    {
        var key = entry switch
        {
            { ReversalOfEntryId: not null } => "Gl_StatusReversal",
            { IsReversed: true } => "Gl_StatusReversed",
            { IsLocked: true } => "Gl_StatusPosted",
            _ => "Gl_StatusDraft",
        };

        return _localization[key];
    }

    private string DescribeIssue(TemplateIssue issue) => string.Format(
        CultureInfo.CurrentCulture,
        _localization["Gl_TemplateIssue_" + issue.Kind],
        issue.Key ?? string.Empty);

    private void ResetLines()
    {
        foreach (var line in Lines)
        {
            line.Changed -= OnLineChanged;
        }

        Lines.Clear();
        Attach(new JournalLineRow());
        Attach(new JournalLineRow());
        Recalculate();
    }

    private void Attach(JournalLineRow line)
    {
        line.Changed += OnLineChanged;
        Lines.Add(line);
    }

    private void OnLineChanged(object? sender, EventArgs e) => Recalculate();

    private IReadOnlyList<JournalLineInput> FilledLines() =>
        [.. Lines.Where(static l => l.Account is not null || l.Debit != 0m || l.Credit != 0m).Select(static l => l.ToInput())];

    private void Recalculate()
    {
        var lines = FilledLines();
        var totals = JournalRules.Totals(lines);
        TotalDebit = totals.Debit;
        TotalCredit = totals.Credit;
        Difference = totals.Difference;
        IsBalanced = totals.IsBalanced;
        BalanceMessage = _localization[totals.IsBalanced ? "Gl_Balanced" : "Gl_Unbalanced"];

        // Post is blocked until the entry is valid and balanced, whatever the amounts say.
        _canPost = !string.IsNullOrWhiteSpace(Description) && JournalRules.Validate([.. lines]).Count == 0;
        PostCommand.NotifyCanExecuteChanged();
    }

    private Task CreateAsync(bool post) => RunAsync(async () =>
    {
        var entry = await _sender.Send(new CreateJournalEntryCommand(PostingDate, Description, SelectedLedger.Book, FilledLines(), post));
        await RefreshEntriesAsync();
        NewEntry();
        StatusMessage = string.Format(CultureInfo.CurrentCulture, _localization[post ? "Gl_EntryPosted" : "Gl_DraftSaved"], entry.EntryNumber);
    });

    private async Task RunAsync(Func<Task> operation)
    {
        IsBusy = true;
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            StatusMessage = JournalMessages.Describe(ex, _localization);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
