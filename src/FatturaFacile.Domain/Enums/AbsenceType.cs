// <copyright file="AbsenceType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Defines the classification of employee leaves and statutory absences under Italian and Swiss employment regulations.
/// </summary>
public enum AbsenceType
{
    /// <summary>
    /// Paid annual vacation leave mandated by statutory labor laws and national collective bargaining agreements (CCNL).
    /// </summary>
    Ferie = 1,

    /// <summary>
    /// Accrued working hours reduction leave (<c>Riduzione Orario di Lavoro</c>) granting paid time off in lieu of contractual weekly hour adjustments.
    /// </summary>
    Rol = 2,

    /// <summary>
    /// Statutory paid permit for assisting family members with severe disabilities or for disabled workers under Italian Law 104/1992.
    /// </summary>
    Legge104 = 3,

    /// <summary>
    /// Temporary medical incapacity certified through the Italian National Social Security Institute (INPS) electronic sickness protocol (<c>PUC</c>).
    /// </summary>
    InpsIllnessPuc = 4,

    /// <summary>
    /// Sickness daily allowance leave under Swiss collective or private daily indemnity insurance (<c>Krankentaggeld / KTG / IPG maladie</c>).
    /// </summary>
    SwissKtgMedical = 5,

    /// <summary>
    /// Swiss mandatory military, civil defense, or alternative civil service leave compensated via the statutory Loss of Earnings Compensation scheme (<c>Erwerbsersatzordnung / EO / APG</c>).
    /// </summary>
    SwissApgMilitaryService = 6,
}
