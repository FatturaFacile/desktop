// <copyright file="WeighingStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Gelato;

/// <summary>
/// Outcome of a weighed ingredient against its target.
/// </summary>
public enum WeighingStatus
{
    /// <summary>The ingredient is not weighed yet.</summary>
    Pending = 0,

    /// <summary>The weight is inside the tolerance of the target.</summary>
    InTolerance = 1,

    /// <summary>The weight is below the target by more than the tolerance.</summary>
    Under = 2,

    /// <summary>The weight is above the target by more than the tolerance.</summary>
    Over = 3,
}
