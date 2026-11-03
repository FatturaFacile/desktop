// <copyright file="EmployeeData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.PayrollHr;

/// <summary>
/// Data transfer object holding identification, contract, and cross-border residence information required to register an employee.
/// </summary>
/// <param name="TenantId">The unique identifier of the employing tenant organization.</param>
/// <param name="Code">The employee internal staff personnel code.</param>
/// <param name="FirstName">The employee's given name.</param>
/// <param name="LastName">The employee's surname.</param>
/// <param name="Dob">The employee's date of birth.</param>
/// <param name="WorkerType">The worker employment category classification.</param>
/// <param name="IsFrontaliere">A flag indicating whether the worker has Italian-Swiss cross-border commuter status.</param>
/// <param name="Address">The residential home address of the employee.</param>
/// <param name="JobTitle">The professional job title.</param>
/// <param name="Salary">The contractual monthly gross salary base.</param>
/// <param name="Currency">The remuneration currency (EUR or CHF).</param>
/// <param name="Canton">The optional two-letter Swiss canton code of employment.</param>
/// <param name="Cf">The optional Italian Codice Fiscale.</param>
/// <param name="Avs">The optional Swiss AHV/AVS social security number (756.xxxx.xxxx.xx).</param>
public sealed record EmployeeData(
    Guid        TenantId,
    string      Code,
    string      FirstName,
    string      LastName,
    DateTime    Dob,
    WorkerType  WorkerType,
    bool        IsFrontaliere,
    Address     Address,
    string      JobTitle,
    decimal     Salary,
    Currency    Currency,
    string?     Canton = null,
    FiscalCode? Cf     = null,
    string?     Avs    = null);
