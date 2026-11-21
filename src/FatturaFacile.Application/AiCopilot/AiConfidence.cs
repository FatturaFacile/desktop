// <copyright file="AiConfidence.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.AiCopilot;

/// <summary>
/// Maps a confidence value to its band.
/// </summary>
public static class AiConfidence
{
    /// <summary>
    /// The minimum confidence of the high band.
    /// </summary>
    public const double HighThreshold = 0.90;

    /// <summary>
    /// The minimum confidence of the medium band.
    /// </summary>
    public const double MediumThreshold = 0.70;

    /// <summary>
    /// Gets the band of a confidence value (0..1).
    /// </summary>
    /// <param name="confidence">The confidence.</param>
    /// <returns>The band.</returns>
    public static AiConfidenceBand BandOf(double confidence) => confidence switch
    {
        >= HighThreshold => AiConfidenceBand.High,
        >= MediumThreshold => AiConfidenceBand.Medium,
        _ => AiConfidenceBand.Low,
    };
}
