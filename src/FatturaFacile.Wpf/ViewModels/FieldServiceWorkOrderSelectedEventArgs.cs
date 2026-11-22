// <copyright file="FieldServiceWorkOrderSelectedEventArgs.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The data of the selection of a work order on the dispatcher map.
/// </summary>
/// <param name="workOrderId">The identifier of the selected work order.</param>
internal sealed class FieldServiceWorkOrderSelectedEventArgs(string workOrderId) : EventArgs
{
    /// <summary>
    /// Gets the identifier of the selected work order.
    /// </summary>
    public string WorkOrderId { get; } = workOrderId;
}
