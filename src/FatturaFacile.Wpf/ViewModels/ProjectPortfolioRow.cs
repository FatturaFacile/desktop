// <copyright file="ProjectPortfolioRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One project of the portfolio overview, formatted for display.
/// </summary>
/// <param name="Id">The project identifier.</param>
/// <param name="Code">The project code.</param>
/// <param name="Name">The project name.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="TypeText">The localized project type.</param>
/// <param name="StartText">The planned start.</param>
/// <param name="EndText">The target end.</param>
/// <param name="Percent">The progress from 0 to 100, for the bar.</param>
/// <param name="PercentText">The formatted progress.</param>
/// <param name="HoursText">The formatted planned hours.</param>
/// <param name="CostText">The formatted planned cost.</param>
/// <param name="RevenueText">The formatted planned revenue.</param>
/// <param name="IsTemplate">Whether the project is a template.</param>
internal sealed record ProjectPortfolioRow(
    Guid Id,
    string Code,
    string Name,
    string StatusText,
    string TypeText,
    string StartText,
    string EndText,
    double Percent,
    string PercentText,
    string HoursText,
    string CostText,
    string RevenueText,
    bool IsTemplate);
