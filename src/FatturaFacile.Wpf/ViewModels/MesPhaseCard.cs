// <copyright file="MesPhaseCard.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One phase on the operator board: the localized texts, the live man / machine / setup / run / downtime counters and which
/// actions the state machine allows. The counters are computed from the seconds of the last read plus the time that passed since,
/// so they keep running between reads, offline included.
/// </summary>
internal sealed partial class MesPhaseCard : ObservableObject
{
    private const double Percent = 100d;

    private readonly ILocalizationService _localization;
    private DateTime _asOfUtc;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _manText = MesDurationFormat.Format(0);

    [ObservableProperty]
    private string _machineText = MesDurationFormat.Format(0);

    [ObservableProperty]
    private string _setupText = MesDurationFormat.Format(0);

    [ObservableProperty]
    private string _runText = MesDurationFormat.Format(0);

    [ObservableProperty]
    private string _downtimeText = MesDurationFormat.Format(0);

    [ObservableProperty]
    private string _openIntervalText = string.Empty;

    [ObservableProperty]
    private string _goodText = "0";

    [ObservableProperty]
    private string _scrapText = "0";

    [ObservableProperty]
    private string _targetText = "0";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _canTakeOver;

    [ObservableProperty]
    private bool _canStartSetup;

    [ObservableProperty]
    private bool _canStartProduction;

    [ObservableProperty]
    private bool _canSuspend;

    [ObservableProperty]
    private bool _canResume;

    [ObservableProperty]
    private bool _canDeclare;

    [ObservableProperty]
    private bool _canConsume;

    [ObservableProperty]
    private bool _canClose;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isSuspended;

    /// <summary>
    /// Initializes a new instance of the <see cref="MesPhaseCard"/> class.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="asOfUtc">The instant the counters of the row are computed for.</param>
    /// <param name="localization">The localization service.</param>
    public MesPhaseCard(MesPhaseRow row, DateTime asOfUtc, ILocalizationService localization)
    {
        _localization = localization;
        Row = row;
        _asOfUtc = asOfUtc;
        Refresh(asOfUtc);
    }

    /// <summary>
    /// Gets the phase as the last read (or local projection) gave it.
    /// </summary>
    public MesPhaseRow Row { get; private set; }

    /// <summary>
    /// Gets the production order number.
    /// </summary>
    public string OrderNumber => Row.OrderNumber;

    /// <summary>
    /// Gets the item being produced.
    /// </summary>
    public string ItemText => Row.ItemCode + " · " + Row.ItemName;

    /// <summary>
    /// Gets the operation, with its sequence.
    /// </summary>
    public string OperationText => string.Create(CultureInfo.InvariantCulture, $"{Row.Sequence} · {Row.OperationName}");

    /// <summary>
    /// Gets the identifier the board matches cards by: the operation, which stays the same when a take-over gives the phase its id.
    /// </summary>
    public Guid OperationId => Row.OperationId;

    /// <summary>
    /// Replaces the phase with a fresh read.
    /// </summary>
    /// <param name="row">The phase.</param>
    /// <param name="asOfUtc">The instant the counters of the row are computed for.</param>
    public void Update(MesPhaseRow row, DateTime asOfUtc)
    {
        Row = row;
        _asOfUtc = asOfUtc;
        OnPropertyChanged(nameof(OrderNumber));
        OnPropertyChanged(nameof(ItemText));
        OnPropertyChanged(nameof(OperationText));
        Refresh(asOfUtc);
    }

    /// <summary>
    /// Advances the counters to an instant.
    /// </summary>
    /// <param name="nowUtc">The current instant.</param>
    public void Tick(DateTime nowUtc) => Refresh(nowUtc);

    private void Refresh(DateTime nowUtc)
    {
        var times = Row.Times.Advance(Row.OpenInterval, (nowUtc - _asOfUtc).TotalSeconds);
        var culture = _localization.CurrentCulture;

        ManText = MesDurationFormat.Format(times.ManSeconds);
        MachineText = MesDurationFormat.Format(times.MachineSeconds);
        SetupText = MesDurationFormat.Format(times.SetupSeconds);
        RunText = MesDurationFormat.Format(times.RunSeconds);
        DowntimeText = MesDurationFormat.Format(times.DowntimeSeconds);
        StatusText = _localization["Mes_Status_" + Row.Status];
        OpenIntervalText = OpenText(Row.OpenInterval);
        GoodText = Row.GoodQuantity.ToString("0.####", culture);
        ScrapText = Row.ScrapQuantity.ToString("0.####", culture);
        TargetText = Row.TargetQuantity.ToString("0.####", culture);
        Progress = Row.TargetQuantity > 0 ? Math.Min(Percent, (double)(Row.GoodQuantity / Row.TargetQuantity) * Percent) : 0d;

        var status = Row.Status;
        CanTakeOver = status is MesPhaseStatus.Available or MesPhaseStatus.Suspended;
        CanStartSetup = MesStateMachine.IsAllowed(MesAction.StartSetup, status);
        CanStartProduction = status is MesPhaseStatus.Assigned or MesPhaseStatus.Setup;
        CanSuspend = MesStateMachine.IsAllowed(MesAction.Suspend, status);
        CanResume = MesStateMachine.IsAllowed(MesAction.Resume, status);
        CanDeclare = MesStateMachine.IsAllowed(MesAction.DeclareProduction, status);
        CanConsume = MesStateMachine.IsAllowed(MesAction.ConsumeMaterial, status);
        CanClose = MesStateMachine.IsAllowed(MesAction.Close, status);
        IsRunning = status == MesPhaseStatus.Running;
        IsSuspended = status == MesPhaseStatus.Suspended;
    }

    private string OpenText(MesOpenInterval? open)
    {
        if (open is null)
        {
            return string.Empty;
        }

        var category = _localization["Mes_Time_" + open.Category];

        return open.Reason is { } reason ? category + " · " + _localization["Mes_Reason_" + reason] : category;
    }
}
