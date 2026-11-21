// <copyright file="PostedTimeInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Costing;

/// <summary>
/// Time confirmed on the shop floor for one operation, used for actual costing.
/// </summary>
/// <param name="SetupMinutes">The setup minutes.</param>
/// <param name="RunMinutes">The run minutes.</param>
/// <param name="LaborMinutes">The labour minutes.</param>
/// <param name="HourlyLaborRate">The labour rate per hour.</param>
/// <param name="HourlyOverheadRate">The overhead rate per machine hour.</param>
public sealed record PostedTimeInput(decimal SetupMinutes, decimal RunMinutes, decimal LaborMinutes, decimal HourlyLaborRate, decimal HourlyOverheadRate);
