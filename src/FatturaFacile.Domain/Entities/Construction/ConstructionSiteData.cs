// <copyright file="ConstructionSiteData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Contains the information required to create a construction site.
/// </summary>
/// <param name="ProjectCode">Unique project code.</param>
/// <param name="Name">Project name.</param>
/// <param name="ClientId">Customer identifier.</param>
/// <param name="SiteAddress">Construction site address.</param>
/// <param name="SiteManagerName">Site manager name.</param>
/// <param name="StartDate">Project start date.</param>
/// <param name="TotalContractBudget">Contract budget.</param>
/// <param name="SafetyCoordinatorName">Safety coordinator name.</param>
public sealed record ConstructionSiteData(
    string ProjectCode,
    string Name,
    Guid ClientId,
    Address SiteAddress,
    string SiteManagerName,
    DateTime StartDate,
    Money TotalContractBudget,
    string? SafetyCoordinatorName = null);
