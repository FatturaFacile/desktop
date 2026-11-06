// <copyright file="TenantChangedNotification.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Notifications;

/// <summary>
/// Domain application notification broadcast when the active tenant context changes.
/// </summary>
/// <param name="TenantId">The unique identifier of the selected tenant.</param>
/// <param name="TenantName">The display name of the tenant company.</param>
/// <param name="CountryCode">The country code ("CH" or "IT").</param>
/// <param name="DefaultCurrency">The default currency ("CHF" or "EUR").</param>
public sealed record TenantChangedNotification(
    Guid TenantId,
    string TenantName,
    string CountryCode,
    string DefaultCurrency) : INotification;
