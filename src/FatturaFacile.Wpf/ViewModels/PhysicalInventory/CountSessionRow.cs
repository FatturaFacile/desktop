// <copyright file="CountSessionRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.InventoryCounting;

namespace FatturaFacile.Wpf.ViewModels.PhysicalInventory;

/// <summary>
/// Display row of a count session with the state rules that enable its actions.
/// </summary>
internal sealed class CountSessionRow
{
    private const string InProgress = "in_progress";

    /// <summary>
    /// Initializes a new instance of the <see cref="CountSessionRow"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="localization">The localization service.</param>
    public CountSessionRow(CountSessionDto session, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(localization);

        Id = session.Id;
        SessionNumber = session.SessionNumber;
        WarehouseId = session.WarehouseId ?? string.Empty;
        CountType = session.CountType;
        CountTypeLabel = CountingText.CountTypeLabel(session.CountType, localization);
        Status = session.Status;
        StatusLabel = CountingText.StatusLabel(session.Status, localization);
        ScheduledDate = session.ScheduledDate ?? string.Empty;
        IsBlind = session.IsBlind;
        IsFrozen = !string.IsNullOrEmpty(session.FrozenAt);
        FreezesStock = session.FreezesStock;
        AssignedTo = session.AssignedTo ?? string.Empty;
        ApprovalLevelRequired = session.ApprovalLevelRequired;
    }

    /// <summary>
    /// Gets the session id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the session number.
    /// </summary>
    public string SessionNumber { get; }

    /// <summary>
    /// Gets the warehouse id.
    /// </summary>
    public string WarehouseId { get; }

    /// <summary>
    /// Gets the count type value.
    /// </summary>
    public string CountType { get; }

    /// <summary>
    /// Gets the localized count type.
    /// </summary>
    public string CountTypeLabel { get; }

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Gets the localized status.
    /// </summary>
    public string StatusLabel { get; }

    /// <summary>
    /// Gets the scheduled date, or an empty text.
    /// </summary>
    public string ScheduledDate { get; }

    /// <summary>
    /// Gets a value indicating whether the count is blind.
    /// </summary>
    public bool IsBlind { get; }

    /// <summary>
    /// Gets a value indicating whether the stock is frozen right now.
    /// </summary>
    public bool IsFrozen { get; }

    /// <summary>
    /// Gets a value indicating whether the session freezes the stock when it runs.
    /// </summary>
    public bool FreezesStock { get; }

    /// <summary>
    /// Gets the assigned counter, or an empty text.
    /// </summary>
    public string AssignedTo { get; }

    /// <summary>
    /// Gets the number of approval levels the session requires, when known.
    /// </summary>
    public int? ApprovalLevelRequired { get; }

    /// <summary>
    /// Gets a value indicating whether the session is still being counted.
    /// </summary>
    public bool IsInProgress => string.Equals(Status, InProgress, StringComparison.Ordinal);

    /// <summary>
    /// Gets a value indicating whether the stock can be frozen (counting, not frozen yet).
    /// </summary>
    public bool CanFreeze => IsInProgress && !IsFrozen;

    /// <summary>
    /// Gets a value indicating whether the freeze can be lifted (counting and frozen).
    /// </summary>
    public bool CanUnfreeze => IsInProgress && IsFrozen;

    /// <summary>
    /// Gets a value indicating whether the session can be assigned, submitted or cancelled (still counting).
    /// </summary>
    public bool CanModify => IsInProgress;
}
