// <copyright file="WhatIfResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Dss;

/// <summary>
/// The result of a simulation.
/// </summary>
/// <param name="Baseline">The figures before the scenario.</param>
/// <param name="Simulated">The figures with the scenario applied.</param>
/// <param name="Delta">The difference.</param>
public sealed record WhatIfResult(WhatIfFigures Baseline, WhatIfFigures Simulated, WhatIfDelta Delta);
