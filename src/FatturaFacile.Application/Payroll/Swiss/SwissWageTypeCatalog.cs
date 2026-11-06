// <copyright file="SwissWageTypeCatalog.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// The standard Swiss wage type catalog created for every Swiss company (editable, system types cannot be deleted).
/// </summary>
public static class SwissWageTypeCatalog
{
    /// <summary>
    /// Creates the standard wage types.
    /// </summary>
    /// <returns>The wage types (without company, assigned on save).</returns>
    public static IReadOnlyList<PayrollWageType> Create()
    {
        return
        [
            Earning(SwissWageTypeCodes.MonthlySalary, "Salario mensile", "1", system: true),
            Earning(SwissWageTypeCodes.HourlyWage, "Salario orario", "1", system: true),
            Earning(SwissWageTypeCodes.HolidayCompensation, "Indennità vacanze (salario orario)", "1", system: true),
            Earning("1010", "Ore straordinarie", "1"),
            Earning("1020", "Indennità turni / notturno / festivi", "1"),
            Earning("1030", "Provvigioni", "1"),
            Irregular("1100", "Gratifica / bonus", "3"),
            Earning(SwissWageTypeCodes.ThirteenthSalary, "13a mensilità", "1", system: true),
            Earning(SwissWageTypeCodes.UnpaidLeave, "Riduzione per congedo non pagato", "1", system: true),
            Allowance("2000", "Indennità giornaliera IGM (perdita di guadagno malattia)"),
            Allowance("2010", "Indennità giornaliera LAINF (infortunio)"),
            Allowance("2020", "Indennità IPG (servizio militare, maternità, paternità)", subjectToAhv: true),
            Family(SwissWageTypeCodes.ChildAllowance, "Assegno per figli"),
            Family(SwissWageTypeCodes.EducationAllowance, "Assegno di formazione"),
            NonCash("4000", "Vitto e alloggio (prestazione in natura)", "2.1"),
            NonCash("4010", "Quota privata auto aziendale", "2.2"),
            NonCash("4020", "Altre prestazioni accessorie", "2.3"),
            Expense("5000", "Rimborso spese effettive", "13.1.1"),
            Expense("5010", "Spese forfettarie di rappresentanza", "13.2.1"),
            Expense("5020", "Spese forfettarie auto", "13.2.2"),
            Expense("5030", "Altre spese forfettarie", "13.2.3"),
            Deduction(SwissWageTypeCodes.Ahv, "Contributo AVS/AI/IPG", "9", 900),
            Deduction(SwissWageTypeCodes.Alv, "Contributo AD (assicurazione disoccupazione)", "9", 910),
            Deduction(SwissWageTypeCodes.Nbu, "Assicurazione infortuni non professionali (AINP)", "9", 920),
            Deduction(SwissWageTypeCodes.Ktg, "Indennità giornaliera di malattia (IGM)", box: null, 930),
            Deduction(SwissWageTypeCodes.Bvg, "Previdenza professionale LPP", "10.1", 940),
            Deduction(SwissWageTypeCodes.WithholdingTax, "Imposta alla fonte", "12", 950),
            Deduction("9060", "Anticipo salario / altre trattenute", box: null, 960, system: false),
            Employer(SwissWageTypeCodes.AhvEmployer, "AVS/AI/IPG datore di lavoro", 1000),
            Employer(SwissWageTypeCodes.AhvAdministration, "Spese amministrative cassa AVS", 1005),
            Employer(SwissWageTypeCodes.AlvEmployer, "AD datore di lavoro", 1010),
            Employer(SwissWageTypeCodes.BuEmployer, "Assicurazione infortuni professionali (AINF)", 1020),
            Employer(SwissWageTypeCodes.KtgEmployer, "IGM datore di lavoro", 1030),
            Employer(SwissWageTypeCodes.BvgEmployer, "LPP datore di lavoro", 1040),
            Employer(SwissWageTypeCodes.FakEmployer, "Cassa assegni familiari (CAF)", 1050),
        ];
    }

    private static PayrollWageType Earning(string code, string name, string box, bool system = false) => new()
    {
        Code = code,
        Name = name,
        Kind = WageTypeKind.Earning,
        SubjectToAhv = true,
        SubjectToAlv = true,
        SubjectToUvg = true,
        SubjectToKtg = true,
        SubjectToBvg = true,
        SubjectToWithholdingTax = true,
        CertificateBox = box,
        SortOrder = int.Parse(code, System.Globalization.CultureInfo.InvariantCulture) / 10,
        IsSystem = system,
    };

    private static PayrollWageType Irregular(string code, string name, string box)
    {
        var type = Earning(code, name, box);
        type.SubjectToBvg = false;
        type.IsIrregular = true;
        return type;
    }

    private static PayrollWageType Allowance(string code, string name, bool subjectToAhv = false)
    {
        var type = Earning(code, name, "7");
        type.SubjectToAhv = subjectToAhv;
        type.SubjectToAlv = subjectToAhv;
        type.SubjectToUvg = false;
        type.SubjectToKtg = false;
        type.SubjectToBvg = false;
        return type;
    }

    private static PayrollWageType Family(string code, string name)
    {
        var type = Allowance(code, name);
        type.CertificateBox = "1";
        type.IsSystem = true;
        return type;
    }

    private static PayrollWageType NonCash(string code, string name, string box)
    {
        var type = Earning(code, name, box);
        type.Kind = WageTypeKind.NonCash;
        type.SubjectToKtg = false;
        type.SubjectToBvg = false;
        return type;
    }

    private static PayrollWageType Expense(string code, string name, string box) => new()
    {
        Code = code,
        Name = name,
        Kind = WageTypeKind.Expense,
        CertificateBox = box,
        SortOrder = int.Parse(code, System.Globalization.CultureInfo.InvariantCulture) / 10,
    };

    private static PayrollWageType Deduction(string code, string name, string? box, int order, bool system = true) => new()
    {
        Code = code,
        Name = name,
        Kind = WageTypeKind.Deduction,
        CertificateBox = box,
        SortOrder = order,
        IsSystem = system,
    };

    private static PayrollWageType Employer(string code, string name, int order) => new()
    {
        Code = code,
        Name = name,
        Kind = WageTypeKind.Employer,
        SortOrder = order,
        IsSystem = true,
    };
}
