// <copyright file="ItalianPayrollEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Constants;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Entities;
using P = FatturaFacile.Application.Strategies.Italian.ItalianPayrollParameters;

namespace FatturaFacile.Application.Payroll.Italian;

/// <summary>
/// Italian monthly payroll: salary and additional installments by collective agreement, INPS (with the 1% above the first
/// pensionable bracket), IRPEF with monthly withholding and year-end adjustment (conguaglio) in December or at termination,
/// regional and municipal surcharges of the previous year in 11 installments, tax wedge, employer INPS and TFR.
/// </summary>
public static class ItalianPayrollEngine
{
    private const decimal ConventionalDays = 26m;
    private const decimal TfrSolidarityRate = 0.005m;
    private static readonly string[] IndemnifiedAbsences = [AbsenceType.Sickness, AbsenceType.Accident, AbsenceType.Maternity];

    /// <summary>
    /// Calculates the payslip of an employee.
    /// </summary>
    /// <param name="context">The payroll input.</param>
    /// <returns>The payslip, or <see langword="null"/> when the employee was not employed in the month.</returns>
    public static ItalianPayrollResult? Calculate(ItalianPayrollContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var employee = context.Employee;
        var first = new DateTime(context.Year, context.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var last = first.AddMonths(1).AddDays(-1);

        if (employee.HireDate > last || (employee.TerminationDate is { } end && end < first))
        {
            return null;
        }

        Builder builder = new(context, first, last);
        builder.Earnings();
        builder.Contributions();
        builder.Taxes();
        builder.EmployerCosts();
        return builder.Build();
    }

    private sealed class Builder(ItalianPayrollContext context, DateTime first, DateTime last)
    {
        private readonly List<PayslipLine> _lines = [];
        private readonly List<string> _warnings = [];
        private readonly HrEmployee _employee = context.Employee;
        private decimal _paidDays = ConventionalDays;
        private decimal _monthly;
        private decimal _inpsBasis;
        private decimal _taxable;

        private int Installments => context.Agreement?.Installments
            ?? (_employee.SalaryInstallmentsPerYear > 0 ? _employee.SalaryInstallmentsPerYear : 13);

        private bool IsYearEnd => context.Month == 12 || (_employee.TerminationDate is { } end && end <= last);

        public void Earnings()
        {
            var fte = _employee.FteRatio > 0 ? _employee.FteRatio : 1m;
            _monthly = Round(_employee.BaseAnnualSalary * fte / Installments);

            if (context.Level is { } level && _monthly < Round(level.MonthlyMinimum * fte))
            {
                _warnings.Add(string.Create(CultureInfo.InvariantCulture, $"Retribuzione mensile {_monthly:0.00} inferiore al minimo del livello {level.Level} ({level.MonthlyMinimum * fte:0.00})."));
            }

            var from = _employee.HireDate > first ? _employee.HireDate.Date : first;
            var to = _employee.TerminationDate is { } end && end < last ? end.Date : last;

            if (from > first || to < last)
            {
                _paidDays = Math.Min(ConventionalDays, WorkingDays(from, to));
                Add(ItalianWageCodes.Salary, "Retribuzione (giorni)", WageTypeKind.Earning, _paidDays, Round(_monthly / ConventionalDays), Round(_monthly * _paidDays / ConventionalDays));
            }
            else
            {
                Add(ItalianWageCodes.Salary, "Retribuzione mensile", WageTypeKind.Earning, 1m, _monthly, _monthly);
            }

            var unpaidDays = context.Absences
                .Where(static a => string.Equals(a.AbsenceType, "Unpaid", StringComparison.Ordinal))
                .Sum(a => WorkingDays(a.StartDate < first ? first : a.StartDate.Date, a.EndDate > last ? last : a.EndDate.Date) * a.Percent / 100m);

            if (unpaidDays > 0m)
            {
                _paidDays = Math.Max(0m, _paidDays - unpaidDays);
                Add(ItalianWageCodes.UnpaidAbsence, "Assenza non retribuita", WageTypeKind.Earning, unpaidDays, -Round(_monthly / ConventionalDays), -Round(_monthly * unpaidDays / ConventionalDays));
            }

            if (context.Absences.Any(static a => IndemnifiedAbsences.Contains(a.AbsenceType, StringComparer.Ordinal)))
            {
                _warnings.Add("Malattia, infortunio o maternità nel mese: indennità INPS/INAIL e integrazioni del CCNL da inserire come voci variabili.");
            }

            AdditionalInstallments();

            foreach (var input in context.Inputs)
            {
                var amount = input.Amount != 0m ? input.Amount : Round(input.Quantity * input.Rate);
                Add(input.WageTypeCode, string.IsNullOrWhiteSpace(input.Note) ? input.WageTypeCode : input.Note, WageTypeKind.Earning, input.Quantity, input.Rate, amount);
            }
        }

        public void Contributions()
        {
            var gross = _lines.Where(static l => WageTypeKind.IsGross(l.Kind)).Sum(static l => l.Amount);
            _inpsBasis = Math.Round(gross, 0, MidpointRounding.AwayFromZero);
            const decimal defaultEmployeeRate = P.InpsEmployeeRate * 100m;
            var rate = context.Settings?.EmployeeInpsRatePercent ?? defaultEmployeeRate;
            var previousBasis = context.YearToDate.Sum(static p => p.AhvBasis);
            var above = Math.Max(0m, previousBasis + _inpsBasis - P.InpsFirstBracket) - Math.Max(0m, previousBasis - P.InpsFirstBracket);
            var inpsEmployee = Round((_inpsBasis * rate / 100m) + (above * P.InpsAdditionalRate));
            Add(ItalianWageCodes.InpsEmployee, "Contributi INPS a carico del dipendente", WageTypeKind.Deduction, _inpsBasis, rate, -inpsEmployee);
            _taxable = gross - inpsEmployee;
        }

        public void Taxes()
        {
            var previousTaxable = context.YearToDate.Sum(static p => p.WithholdingTaxBasis);
            var previousIrpef = -YearToDate(ItalianWageCodes.Irpef);
            var previousWedge = YearToDate(ItalianWageCodes.TaxWedge);
            decimal irpef;
            decimal wedge;

            if (IsYearEnd)
            {
                var annual = previousTaxable + _taxable;
                irpef = AnnualIrpef(annual) - previousIrpef;
                wedge = Round(P.ComputeTaxWedgeBonus(annual)) - previousWedge;
                _warnings.Add("Conguaglio IRPEF di fine anno o di cessazione applicato (senza detrazioni per familiari e altri redditi).");
            }
            else
            {
                var projected = _taxable * 12m;
                irpef = Round(AnnualIrpef(projected) / 12m);
                wedge = Round(P.ComputeTaxWedgeBonus(projected) / 12m);
            }

            Add(ItalianWageCodes.Irpef, irpef >= 0m ? "Ritenuta IRPEF" : "Rimborso IRPEF da conguaglio", WageTypeKind.Deduction, _taxable, 0m, -irpef);

            if (wedge != 0m)
            {
                Add(ItalianWageCodes.TaxWedge, "Somma non imponibile cuneo fiscale", WageTypeKind.Expense, 1m, 0m, wedge);
            }

            if (context.Month <= 11 && context.PreviousYearTaxable > 0m)
            {
                var regional = Round(context.PreviousYearTaxable * _employee.RegionalSurchargeRatePercent / 100m / 11m);
                var municipal = Round(context.PreviousYearTaxable * _employee.MunicipalSurchargeRatePercent / 100m / 11m);
                Add(ItalianWageCodes.RegionalSurcharge, "Addizionale regionale anno precedente (rata)", WageTypeKind.Deduction, context.PreviousYearTaxable, _employee.RegionalSurchargeRatePercent, -regional);
                Add(ItalianWageCodes.MunicipalSurcharge, "Addizionale comunale anno precedente (rata)", WageTypeKind.Deduction, context.PreviousYearTaxable, _employee.MunicipalSurchargeRatePercent, -municipal);
            }
        }

        public void EmployerCosts()
        {
            const decimal defaultEmployerRate = P.IndicativeEmployerInpsRate * 100m;
            var rate = context.Agreement?.EmployerInpsRatePercent ?? context.Settings?.EmployerInpsRatePercent ?? defaultEmployerRate;
            Add(ItalianWageCodes.InpsEmployer, "Contributi INPS a carico dell'azienda", WageTypeKind.Employer, _inpsBasis, rate, Round(_inpsBasis * rate / 100m));
            var tfrBasis = _lines.Where(static l => WageTypeKind.IsGross(l.Kind)).Sum(static l => l.Amount);
            Add(ItalianWageCodes.Tfr, "Quota TFR maturata (al netto dello 0,50%)", WageTypeKind.Employer, tfrBasis, 0m, Round((tfrBasis / P.TfrDivisor) - (_inpsBasis * TfrSolidarityRate)));
        }

        public ItalianPayrollResult Build()
        {
            var gross = _lines.Where(static l => WageTypeKind.IsGross(l.Kind)).Sum(static l => l.Amount);
            var deductions = -_lines.Where(static l => string.Equals(l.Kind, WageTypeKind.Deduction, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var additions = _lines.Where(static l => string.Equals(l.Kind, WageTypeKind.Expense, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var employer = _lines.Where(static l => string.Equals(l.Kind, WageTypeKind.Employer, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var irpef = -_lines.Where(static l => string.Equals(l.WageTypeCode, ItalianWageCodes.Irpef, StringComparison.Ordinal)).Sum(static l => l.Amount);

            Payslip payslip = new()
            {
                EmployeeId = _employee.Id,
                Year = context.Year,
                Month = context.Month,
                EmployeeName = _employee.FullName,
                PaidDays = _paidDays,
                GrossSalary = gross,
                AhvBasis = _inpsBasis,
                WithholdingTaxBasis = _taxable,
                WithholdingTaxRatePercent = _taxable > 0m ? Round(irpef / _taxable * 100m) : 0m,
                EmployeeDeductions = deductions,
                NetSalary = gross - deductions,
                PayoutAmount = gross - deductions + additions,
                EmployerContributions = employer,
                Warnings = _warnings.Count == 0 ? null : string.Join(Environment.NewLine, _warnings),
            };

            foreach (var line in _lines)
            {
                line.PayslipId = payslip.Id;
            }

            return new ItalianPayrollResult(payslip, _lines);
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static int WorkingDays(DateTime from, DateTime to)
        {
            var days = 0;

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                if (day.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
            }

            return days;
        }

        private static decimal AnnualIrpef(decimal income) =>
            Round(Math.Max(0m, P.ComputeGrossIrpef(income) - P.ComputeWorkDeduction(income) - P.ComputeTaxWedgeDeduction(income)));

        private decimal YearToDate(string code) =>
            context.YearToDateLines.Where(l => string.Equals(l.WageTypeCode, code, StringComparison.Ordinal)).Sum(static l => l.Amount);

        private void AdditionalInstallments()
        {
            var monthsInYear = MonthsEmployed(new DateTime(context.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

            if (Installments >= 13 && IsYearEnd)
            {
                var amount = Round(_monthly * monthsInYear / 12m);
                Add(ItalianWageCodes.Thirteenth, "Tredicesima mensilità", WageTypeKind.Earning, monthsInYear, Round(_monthly / 12m), amount);
            }

            var fourteenthMonth = context.Agreement?.FourteenthMonth ?? 7;

            if (Installments >= 14 && context.Month == fourteenthMonth)
            {
                var months = MonthsEmployed(new DateTime(context.Year, fourteenthMonth, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(-12));
                Add(ItalianWageCodes.Fourteenth, "Quattordicesima mensilità", WageTypeKind.Earning, months, Round(_monthly / 12m), Round(_monthly * months / 12m));
            }
        }

        private int MonthsEmployed(DateTime periodStart)
        {
            var count = 0;

            for (var month = periodStart; month <= first; month = month.AddMonths(1))
            {
                var monthEnd = month.AddMonths(1).AddDays(-1);
                var employed = _employee.HireDate <= monthEnd.AddDays(-14) && (_employee.TerminationDate is not { } end || end >= month.AddDays(14));

                if (employed)
                {
                    count++;
                }
            }

            return Math.Min(12, count);
        }

        private void Add(string code, string description, string kind, decimal quantity, decimal rate, decimal amount) =>
            _lines.Add(new PayslipLine
            {
                WageTypeCode = code,
                Description = description,
                Kind = kind,
                Quantity = quantity,
                Rate = rate,
                Amount = Round(amount),
                SortOrder = _lines.Count,
            });
    }
}
