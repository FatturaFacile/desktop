// <copyright file="TransportDocumentData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

/// <summary>
/// Data transfer object holding logistics, carrier, and destination information required to create a transport document (DDT).
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="Number">The document sequence number.</param>
/// <param name="Date">The dispatch date of the goods.</param>
/// <param name="ClientId">The consignee client identifier.</param>
/// <param name="Destination">The delivery destination address.</param>
/// <param name="Reason">The statutory transportation reason (Causale di trasporto).</param>
/// <param name="Carrier">The freight carrier name or conveyance designation.</param>
/// <param name="Packages">The number of parcels or collis.</param>
/// <param name="WeightKg">The gross cargo weight in kilograms.</param>
/// <param name="CustomsCmr">Optional cross-border customs declarations or CMR transport remarks.</param>
public sealed record TransportDocumentData(
    Guid     TenantId,
    string   Number,
    DateTime Date,
    Guid     ClientId,
    Address  Destination,
    string   Reason,
    string   Carrier,
    int      Packages,
    decimal  WeightKg,
    string?  CustomsCmr = null);
