// <copyright file="FieldServiceCacheKinds.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The kinds of read model kept in the local Field Service cache.
/// </summary>
public static class FieldServiceCacheKinds
{
    /// <summary>
    /// The dispatcher board (key <see cref="All"/>): the last window pulled.
    /// </summary>
    public const string Board = "board";

    /// <summary>
    /// The maintenance contracts (key <see cref="All"/>).
    /// </summary>
    public const string Contracts = "contracts";

    /// <summary>
    /// The customer equipment (key <see cref="All"/>).
    /// </summary>
    public const string Equipment = "equipment";

    /// <summary>
    /// The list of work orders (key <see cref="All"/>).
    /// </summary>
    public const string WorkOrders = "work_orders";

    /// <summary>
    /// One work order with its parts and report (key: the work order identifier).
    /// </summary>
    public const string WorkOrder = "work_order";

    /// <summary>
    /// The service history of an asset (key: the equipment identifier).
    /// </summary>
    public const string History = "history";

    /// <summary>
    /// The offline bundle of a technician (key: the technician identifier).
    /// </summary>
    public const string Bundle = "bundle";

    /// <summary>
    /// The key of the singleton kinds.
    /// </summary>
    public const string All = "all";
}
