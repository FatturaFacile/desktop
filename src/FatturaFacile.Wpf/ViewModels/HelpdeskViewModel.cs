// <copyright file="HelpdeskViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Helpdesk;
using FatturaFacile.Application.Helpdesk;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the support ticket workbench (Mod 083): the tickets sorted by SLA urgency with breach and at-risk indicators, filters, the
/// customer 360 panel, the escalation history, the knowledge base lookup, the time log and the escalation actions. It works from
/// the cache offline and queues time logs and escalations until the server is reachable.
/// </summary>
internal sealed partial class HelpdeskViewModel : ObservableObject
{
    private const int DefaultRiskPercent = 75;
    private const string DateTimeFormat = "g";
    private const string Breached = "Breached";
    private const string AtRisk = "AtRisk";
    private const string AnyKey = "";

    private readonly ISender _sender;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _time;

    private IReadOnlyList<HelpdeskTicketRisk> _tickets = [];
    private HashSet<Guid> _pendingTickets = [];
    private HelpdeskCustomerContext? _context;
    private bool _suppressFilter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private HelpdeskChoice? _selectedPriority;

    [ObservableProperty]
    private HelpdeskChoice? _selectedState;

    [ObservableProperty]
    private int _minRiskPercent = DefaultRiskPercent;

    [ObservableProperty]
    private HelpdeskTicketRow? _selectedTicket;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isOffline;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _lastSyncedText = string.Empty;

    [ObservableProperty]
    private bool _hasPending;

    [ObservableProperty]
    private bool _hasConflicts;

    [ObservableProperty]
    private bool _hasContext;

    [ObservableProperty]
    private string _invoicesText = string.Empty;

    [ObservableProperty]
    private string _contextNote = string.Empty;

    [ObservableProperty]
    private bool _hasEscalations;

    [ObservableProperty]
    private string _knowledgeQuery = string.Empty;

    [ObservableProperty]
    private string _knowledgeStatus = string.Empty;

    [ObservableProperty]
    private HelpdeskTimeLogViewModel? _timeLog;

    [ObservableProperty]
    private HelpdeskEscalationViewModel? _escalation;

    [ObservableProperty]
    private bool _hasSelection;

    [ObservableProperty]
    private bool _isTimeLogOpen;

    [ObservableProperty]
    private bool _isEscalationOpen;

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpdeskViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localization">The localization service.</param>
    /// <param name="time">The clock.</param>
    public HelpdeskViewModel(ISender sender, ILocalizationService localization, TimeProvider time)
    {
        _sender = sender;
        _localization = localization;
        _time = time;
        StateChoices =
        [
            new HelpdeskChoice(AnyKey, _localization["Helpdesk_All"]),
            new HelpdeskChoice(Breached, _localization["Helpdesk_StateBreached"]),
            new HelpdeskChoice(AtRisk, _localization["Helpdesk_StateAtRisk"]),
        ];
        _selectedState = StateChoices[0];
        PriorityChoices.Add(new HelpdeskChoice(AnyKey, _localization["Helpdesk_All"]));
        _selectedPriority = PriorityChoices[0];
        LastSyncedText = _localization["Helpdesk_NeverSynced"];
    }

    /// <summary>
    /// Gets the SLA state filter entries.
    /// </summary>
    public IReadOnlyList<HelpdeskChoice> StateChoices { get; }

    /// <summary>
    /// Gets the priority filter entries: all, then the priorities of the loaded tickets.
    /// </summary>
    public ObservableCollection<HelpdeskChoice> PriorityChoices { get; } = [];

    /// <summary>
    /// Gets the knowledge base articles found for the query.
    /// </summary>
    public ObservableCollection<HelpdeskKbArticle> KnowledgeResults { get; } = [];

    /// <summary>
    /// Gets the tickets that pass the filters, most urgent first.
    /// </summary>
    internal ObservableCollection<HelpdeskTicketRow> Rows { get; } = [];

    /// <summary>
    /// Gets the actions waiting for the server.
    /// </summary>
    internal ObservableCollection<HelpdeskQueueRow> Queue { get; } = [];

    /// <summary>
    /// Gets the actions the server refused.
    /// </summary>
    internal ObservableCollection<HelpdeskQueueRow> Conflicts { get; } = [];

    /// <summary>
    /// Gets the contracts of the customer of the selected ticket.
    /// </summary>
    internal ObservableCollection<HelpdeskContractRow> Contracts { get; } = [];

    /// <summary>
    /// Gets the assets of the customer of the selected ticket.
    /// </summary>
    internal ObservableCollection<HelpdeskAssetRow> Assets { get; } = [];

    /// <summary>
    /// Gets the recent tickets of the customer of the selected ticket.
    /// </summary>
    internal ObservableCollection<HelpdeskTextRow> RecentTickets { get; } = [];

    /// <summary>
    /// Gets the SLA escalation steps that fired on the selected ticket.
    /// </summary>
    internal ObservableCollection<HelpdeskTextRow> Escalations { get; } = [];

    /// <summary>
    /// Sends the queued actions, then pulls the breach-risk list; offline, the last synced list stays on screen.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var workbench = await _sender.Send(new GetHelpdeskWorkbenchQuery(Math.Clamp(MinRiskPercent, 0, 100)));
            Apply(workbench);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Searches the knowledge base in the language of the application.
    /// </summary>
    /// <returns>A task representing the search.</returns>
    [RelayCommand]
    public async Task SearchKnowledgeAsync()
    {
        IsBusy = true;

        try
        {
            var locale = HelpdeskKnowledgeLocale.From(_localization.CurrentCulture);
            var load = await _sender.Send(new SearchHelpdeskKnowledgeQuery(KnowledgeQuery, locale));
            KnowledgeResults.Clear();

            if (load is null)
            {
                KnowledgeStatus = _localization["Helpdesk_KbOffline"];

                return;
            }

            foreach (var article in load.Data)
            {
                KnowledgeResults.Add(article);
            }

            KnowledgeStatus = (load.FromCache, load.Data.Count) switch
            {
                (true, _) => _localization["Helpdesk_KbCached"],
                (false, 0) => _localization["Helpdesk_KbNoResults"],
                _ => string.Empty,
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            KnowledgeStatus = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string UrgencyState(HelpdeskTicketRisk ticket) => ticket.IsBreached ? Breached : AtRisk;

    private static bool Matches(HelpdeskTicketRisk ticket, string text, string priority, string state) =>
        MatchesPriority(ticket, priority) && MatchesState(ticket, state) && (text.Length == 0 || ContainsText(ticket, text));

    private static bool MatchesPriority(HelpdeskTicketRisk ticket, string priority) =>
        priority.Length == 0 || string.Equals(ticket.Priority, priority, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesState(HelpdeskTicketRisk ticket, string state) =>
        state.Length == 0 || string.Equals(UrgencyState(ticket), state, StringComparison.Ordinal);

    private static bool ContainsText(HelpdeskTicketRisk ticket, string text) =>
        ticket.Subject.Contains(text, StringComparison.CurrentCultureIgnoreCase)
        || ticket.TicketNumber.Contains(text, StringComparison.CurrentCultureIgnoreCase);

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnMinRiskPercentChanged(int value) => ApplyFilters();

    partial void OnSelectedPriorityChanged(HelpdeskChoice? value) => ApplyFilters();

    partial void OnSelectedStateChanged(HelpdeskChoice? value) => ApplyFilters();

    partial void OnTimeLogChanged(HelpdeskTimeLogViewModel? value) => IsTimeLogOpen = value is not null;

    partial void OnEscalationChanged(HelpdeskEscalationViewModel? value) => IsEscalationOpen = value is not null;

    partial void OnSelectedTicketChanged(HelpdeskTicketRow? value)
    {
        HasSelection = value is not null;
        TimeLog = null;
        Escalation = null;
        if (value is null)
        {
            ClearDetail();

            return;
        }

        KnowledgeQuery = value.Subject;
        _ = LoadDetailAsync(value.Ticket);
    }

    [RelayCommand]
    private async Task FlushAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _sender.Send(new FlushHelpdeskOutboxCommand());
            StatusMessage = result.Offline
                ? _localization["Helpdesk_OfflineCache"]
                : string.Format(_localization.CurrentCulture, _localization["Helpdesk_FlushResult"], result.Sent.ToString(_localization.CurrentCulture));
            var workbench = await _sender.Send(new GetHelpdeskWorkbenchQuery(Math.Clamp(MinRiskPercent, 0, 100)));
            Apply(workbench);
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DismissAsync(HelpdeskQueueRow? row)
    {
        if (row is null)
        {
            return;
        }

        try
        {
            await _sender.Send(new DismissHelpdeskConflictCommand(row.Id));
            await LoadAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void BeginTimeLog()
    {
        if (SelectedTicket is { } row)
        {
            TimeLog = new HelpdeskTimeLogViewModel(_sender, _localization, _time, row.Ticket, _context?.RemainingContractHours, LoadAsync);
        }
    }

    [RelayCommand]
    private void CloseTimeLog() => TimeLog = null;

    [RelayCommand]
    private void BeginEscalation()
    {
        if (SelectedTicket is { } row)
        {
            Escalation = new HelpdeskEscalationViewModel(
                _sender,
                _localization,
                _time,
                row.Ticket,
                _context?.Assets.Select(asset => asset.SerialNumber).Where(serial => serial.Length > 0) ?? [],
                LoadAsync);
        }
    }

    [RelayCommand]
    private void CloseEscalation() => Escalation = null;

    private void Apply(HelpdeskWorkbench workbench)
    {
        _tickets = workbench.Tickets;
        _pendingTickets = [.. workbench.Pending.Select(action => action.TicketId)];
        IsOffline = workbench.FromCache || workbench.SyncedAtUtc is null;
        var culture = _localization.CurrentCulture;
        LastSyncedText = workbench.SyncedAtUtc is { } at
            ? string.Format(culture, _localization["Helpdesk_LastSynced"], at.ToLocalTime().ToString(DateTimeFormat, culture))
            : _localization["Helpdesk_NeverSynced"];
        StatusMessage = IsOffline ? _localization["Helpdesk_OfflineCache"] : string.Empty;

        Show(Queue, workbench.Pending);
        Show(Conflicts, workbench.Conflicts);
        HasPending = Queue.Count > 0;
        HasConflicts = Conflicts.Count > 0;
        RefreshPriorities();
        ApplyFilters();
    }

    private void Show(ObservableCollection<HelpdeskQueueRow> target, IReadOnlyList<HelpdeskPendingAction> actions)
    {
        target.Clear();
        foreach (var action in actions)
        {
            target.Add(new HelpdeskQueueRow(
                action.Id,
                _localization["Helpdesk_Action_" + action.Action],
                action.TicketNumber,
                action.OccurredAtUtc.ToLocalTime().ToString(DateTimeFormat, _localization.CurrentCulture),
                action.Error));
        }
    }

    private void RefreshPriorities()
    {
        var keep = SelectedPriority?.Key ?? AnyKey;
        _suppressFilter = true;
        try
        {
            PriorityChoices.Clear();
            PriorityChoices.Add(new HelpdeskChoice(AnyKey, _localization["Helpdesk_All"]));
            foreach (var priority in _tickets.Select(ticket => ticket.Priority).Where(priority => priority.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase))
            {
                PriorityChoices.Add(new HelpdeskChoice(priority, priority));
            }

            SelectedPriority = PriorityChoices.FirstOrDefault(choice => string.Equals(choice.Key, keep, StringComparison.OrdinalIgnoreCase)) ?? PriorityChoices[0];
        }
        finally
        {
            _suppressFilter = false;
        }
    }

    private void ApplyFilters()
    {
        if (_suppressFilter)
        {
            return;
        }

        var keep = SelectedTicket?.Ticket.TicketId;
        var text = SearchText.Trim();
        var priority = SelectedPriority?.Key ?? AnyKey;
        var state = SelectedState?.Key ?? AnyKey;

        var visible = _tickets.Where(ticket => ticket.RiskPercent >= MinRiskPercent && Matches(ticket, text, priority, state));

        Rows.Clear();
        foreach (var ticket in visible)
        {
            Rows.Add(ToRow(ticket));
        }

        SelectedTicket = keep is { } id ? Rows.FirstOrDefault(row => row.Ticket.TicketId == id) : null;
        if (!IsOffline && StatusMessage.Length == 0 && Rows.Count == 0)
        {
            StatusMessage = _localization["Helpdesk_NoTickets"];
        }
    }

    private HelpdeskTicketRow ToRow(HelpdeskTicketRisk ticket)
    {
        var culture = _localization.CurrentCulture;
        var due = ticket.EarliestDueAt;

        return new HelpdeskTicketRow(
            ticket,
            ticket.RiskPercent.ToString("0", culture) + "%",
            UrgencyState(ticket),
            due is { } instant ? DateTime.SpecifyKind(instant, DateTimeKind.Utc).ToLocalTime().ToString(DateTimeFormat, culture) : "-",
            string.Join(", ", ticket.Targets.Select(target => TargetName(target.Target)).Distinct(StringComparer.Ordinal)),
            _pendingTickets.Contains(ticket.TicketId));
    }

    private string TargetName(string target) => target.Length == 0 ? "-" : _localization["Helpdesk_Target_" + target];

    private void ClearDetail()
    {
        _context = null;
        HasContext = false;
        ContextNote = string.Empty;
        InvoicesText = string.Empty;
        HasEscalations = false;
        Contracts.Clear();
        Assets.Clear();
        RecentTickets.Clear();
        Escalations.Clear();
    }

    private async Task LoadDetailAsync(HelpdeskTicketRisk ticket)
    {
        try
        {
            var context = await _sender.Send(new GetHelpdeskCustomerContextQuery(ticket.TicketId));
            var escalations = await _sender.Send(new GetHelpdeskEscalationsQuery(ticket.TicketId));

            // The selection moved on while the answers were on their way.
            if (SelectedTicket?.Ticket.TicketId != ticket.TicketId)
            {
                return;
            }

            ShowContext(context);
            ShowEscalations(escalations);
        }
        catch (UnauthorizedAccessException ex)
        {
            ContextNote = ex.Message;
        }
    }

    private void ShowContext(HelpdeskLoad<HelpdeskCustomerContext>? load)
    {
        Contracts.Clear();
        Assets.Clear();
        RecentTickets.Clear();
        _context = load?.Data;
        HasContext = load is not null;
        ContextNote = load switch
        {
            null => _localization["Helpdesk_NoContext"],
            { FromCache: true } => _localization["Helpdesk_ContextCached"],
            _ => string.Empty,
        };

        if (load is null)
        {
            InvoicesText = string.Empty;

            return;
        }

        var culture = _localization.CurrentCulture;
        var today = DateOnly.FromDateTime(_time.GetLocalNow().Date);
        var context = load.Data;

        foreach (var contract in context.Contracts)
        {
            Contracts.Add(new HelpdeskContractRow(contract.Name, BankText(contract), contract.RemainingHours is <= 0m));
        }

        foreach (var asset in context.Assets)
        {
            var expired = asset.WarrantyExpiryDate is { } end && end < today;
            Assets.Add(new HelpdeskAssetRow(asset.SerialNumber, WarrantyText(asset, expired), expired));
        }

        foreach (var recent in context.RecentTickets)
        {
            RecentTickets.Add(new HelpdeskTextRow(recent.Number + " " + recent.Subject, recent.Status));
        }

        var invoices = context.OpenInvoices;
        var open = invoices.Count.ToString(culture);
        var overdue = invoices.OverdueCount.ToString(culture);
        var total = invoices.TotalGross.ToString("N2", culture);
        InvoicesText = string.Format(culture, _localization["Helpdesk_InvoicesText"], open, overdue, total);
    }

    private string BankText(HelpdeskContract contract)
    {
        if (contract.RemainingHours is not { } left)
        {
            return _localization["Helpdesk_ContractNoBank"];
        }

        var culture = _localization.CurrentCulture;

        return string.Format(culture, _localization["Helpdesk_ContractBank"], left.ToString("0.##", culture), (contract.IncludedHours ?? left).ToString("0.##", culture));
    }

    private string WarrantyText(HelpdeskAsset asset, bool expired)
    {
        var key = expired ? "Helpdesk_WarrantyExpired" : "Helpdesk_Warranty";
        var culture = _localization.CurrentCulture;

        return asset.WarrantyExpiryDate is { } date
            ? string.Format(culture, _localization[key], date.ToString("d", culture))
            : _localization["Helpdesk_NoWarranty"];
    }

    private void ShowEscalations(HelpdeskLoad<IReadOnlyList<HelpdeskEscalation>>? load)
    {
        Escalations.Clear();
        var culture = _localization.CurrentCulture;

        foreach (var step in load?.Data ?? [])
        {
            Escalations.Add(new HelpdeskTextRow(
                string.Format(culture, _localization["Helpdesk_EscalationLine"], TargetName(step.Target), step.Action, step.ElapsedPercent.ToString("0", culture)),
                step.FiredAt is { } at ? at.ToLocalTime().ToString(DateTimeFormat, culture) + " " + step.Outcome : step.Outcome));
        }

        HasEscalations = Escalations.Count > 0;
    }
}
