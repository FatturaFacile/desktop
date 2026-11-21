// <copyright file="AiInsightRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Display row of a predictive insight (anomaly alert or recommendation).
/// </summary>
/// <param name="Heading">The localized category (anomaly or recommendation).</param>
/// <param name="Title">The localized kind and source of the insight.</param>
/// <param name="Detail">The reasoning of the model.</param>
/// <param name="BandLabel">The localized confidence band.</param>
internal sealed record AiInsightRow(string Heading, string Title, string Detail, string BandLabel);
