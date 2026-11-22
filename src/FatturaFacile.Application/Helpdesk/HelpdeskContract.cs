// <copyright file="HelpdeskContract.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Helpdesk;

/// <summary>
/// A service contract of the customer with its hour bank.
/// </summary>
/// <param name="Id">The server identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Kind">The kind of contract.</param>
/// <param name="IncludedHours">The hours included, when the contract has a bank.</param>
/// <param name="ConsumedHours">The hours consumed.</param>
/// <param name="RemainingHours">The hours left, when the contract has a bank.</param>
/// <param name="EndDate">The end date, when any.</param>
public sealed record HelpdeskContract(Guid? Id, string Name, string Kind, decimal? IncludedHours, decimal? ConsumedHours, decimal? RemainingHours, DateOnly? EndDate);
