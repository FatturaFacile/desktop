// <copyright file="FieldServiceSyncScope.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Which read model a sync pulls into the local cache.
/// </summary>
public enum FieldServiceSyncScope
{
    /// <summary>
    /// The dispatcher board of a window.
    /// </summary>
    Board = 0,

    /// <summary>
    /// The maintenance contracts.
    /// </summary>
    Contracts = 1,

    /// <summary>
    /// The customer equipment.
    /// </summary>
    Equipment = 2,

    /// <summary>
    /// The list of work orders.
    /// </summary>
    WorkOrders = 3,

    /// <summary>
    /// One work order with parts and report; the key is its identifier.
    /// </summary>
    WorkOrder = 4,

    /// <summary>
    /// The service history of an asset; the key is the equipment identifier.
    /// </summary>
    History = 5,

    /// <summary>
    /// The offline bundle of a technician; the key is the technician identifier.
    /// </summary>
    Bundle = 6,
}
