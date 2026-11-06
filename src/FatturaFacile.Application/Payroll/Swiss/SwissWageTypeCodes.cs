// <copyright file="SwissWageTypeCodes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// The wage type codes used by the calculation engine.
/// </summary>
public static class SwissWageTypeCodes
{
    /// <summary>Monthly salary.</summary>
    public const string MonthlySalary = "1000";

    /// <summary>Hourly wage (input: hours).</summary>
    public const string HourlyWage = "1005";

    /// <summary>Holiday compensation of hourly paid employees.</summary>
    public const string HolidayCompensation = "1006";

    /// <summary>13th monthly salary.</summary>
    public const string ThirteenthSalary = "1200";

    /// <summary>Salary reduction for unpaid leave.</summary>
    public const string UnpaidLeave = "1900";

    /// <summary>Child allowance.</summary>
    public const string ChildAllowance = "3000";

    /// <summary>Education allowance.</summary>
    public const string EducationAllowance = "3010";

    /// <summary>AHV/IV/EO employee contribution.</summary>
    public const string Ahv = "9000";

    /// <summary>ALV employee contribution.</summary>
    public const string Alv = "9010";

    /// <summary>Non-occupational accident insurance (NBU).</summary>
    public const string Nbu = "9020";

    /// <summary>Daily sickness insurance (employee).</summary>
    public const string Ktg = "9030";

    /// <summary>BVG employee contribution.</summary>
    public const string Bvg = "9040";

    /// <summary>Withholding tax.</summary>
    public const string WithholdingTax = "9050";

    /// <summary>AHV/IV/EO employer contribution.</summary>
    public const string AhvEmployer = "9500";

    /// <summary>AHV administration costs.</summary>
    public const string AhvAdministration = "9505";

    /// <summary>ALV employer contribution.</summary>
    public const string AlvEmployer = "9510";

    /// <summary>Occupational accident insurance (BU).</summary>
    public const string BuEmployer = "9520";

    /// <summary>Daily sickness insurance (employer).</summary>
    public const string KtgEmployer = "9530";

    /// <summary>BVG employer contribution.</summary>
    public const string BvgEmployer = "9540";

    /// <summary>Family allowance fund contribution.</summary>
    public const string FakEmployer = "9550";
}
