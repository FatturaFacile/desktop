// <copyright file="HelpdeskAsset.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// An installed asset of the customer.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="SerialNumber">The serial number.</param>
/// <param name="WarrantyExpiryDate">The end of the warranty, when known.</param>
public sealed record HelpdeskAsset(Guid? Id, string SerialNumber, DateOnly? WarrantyExpiryDate);
