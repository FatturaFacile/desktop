// <copyright file="SwissPayrollEngine.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Payroll.Swiss;

/// <summary>
/// Swiss monthly payroll calculation driven by the wage type catalog: every line contributes to the bases
/// (AHV, ALV, UVG, KTG, BVG, withholding tax) according to the flags of its wage type.
/// </summary>
public static class SwissPayrollEngine
{
    private const decimal DaysPerMonth = 30m;
    private const decimal DaysPerYear = 360m;

    /// <summary>
    /// Calculates the payslip of an employee for a month.
    /// </summary>
    /// <param name="context">The calculation context.</param>
    /// <returns>The result, or null when the employee is not employed in the month.</returns>
    public static SwissPayrollResult? Calculate(SwissPayrollContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var employee = context.Employee;
        var paidDays = SwissPayrollRules.PaidDays(context.Year, context.Month, employee.HireDate, employee.TerminationDate);

        if (paidDays <= 0m)
        {
            return null;
        }

        SwissPayrollCalculation calculation = new(context, paidDays);
        calculation.AddEarnings();
        calculation.AddInputs();
        calculation.AddFamilyAllowances();
        calculation.ComputeBases();
        calculation.AddContributions();
        calculation.AddWithholdingTax();
        return calculation.Build();
    }

    /// <summary>
    /// The state of one calculation.
    /// </summary>
    /// <param name="context">The calculation context.</param>
    /// <param name="paidDays">The paid days of the month.</param>
    private sealed class SwissPayrollCalculation(SwissPayrollContext context, decimal paidDays)
    {
        private readonly List<PayslipLine> _lines = [];
        private readonly List<string> _warnings = [];
        private readonly HrEmployee _employee = context.Employee;
        private readonly SwissPayrollSettings _settings = context.Settings;
        private readonly DateTime _periodStart = new(context.Year, context.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);

        private decimal _ahvBasis;
        private decimal _alvBasis;
        private decimal _uvgBasis;
        private decimal _ktgBasis;
        private decimal _bvgCoordinated;
        private decimal _qstBasis;
        private decimal _qstRate;
        private string? _qstCanton;

        private DateTime PeriodEnd => _periodStart.AddMonths(1).AddDays(-1);

        private bool IsHourly => string.Equals(_employee.PayType, "hourly", StringComparison.OrdinalIgnoreCase);

        private decimal MonthlySalary => _employee.SalaryInstallmentsPerYear <= 0
            ? 0m
            : _employee.BaseAnnualSalary / _employee.SalaryInstallmentsPerYear;

        public void AddEarnings()
        {
            if (IsHourly)
            {
                return;
            }

            var monthly = MonthlySalary;

            if (paidDays >= DaysPerMonth)
            {
                AddLine(SwissWageTypeCodes.MonthlySalary, 1m, monthly, monthly);
            }
            else
            {
                AddLine(SwissWageTypeCodes.MonthlySalary, paidDays, monthly / DaysPerMonth, monthly * paidDays / DaysPerMonth);
            }

            var unpaidDays = context.Absences
                .Where(static a => string.Equals(a.AbsenceType, "Unpaid", StringComparison.OrdinalIgnoreCase))
                .Sum(a => SwissPayrollRules.AbsenceDays(context.Year, context.Month, a.StartDate, a.EndDate, a.Percent));

            if (unpaidDays > 0m)
            {
                var days = Math.Min(unpaidDays, paidDays);
                AddLine(SwissWageTypeCodes.UnpaidLeave, days, -monthly / DaysPerMonth, -monthly * days / DaysPerMonth);
            }

            var exitMonth = _employee.TerminationDate is { } termination && termination.Year == context.Year && termination.Month == context.Month;

            if (_employee.SalaryInstallmentsPerYear == 13 && (context.Month == 12 || exitMonth))
            {
                var yearDays = Math.Min(context.YearToDate.PaidDays + paidDays, DaysPerYear);
                AddLine(SwissWageTypeCodes.ThirteenthSalary, yearDays, monthly / DaysPerYear, monthly * yearDays / DaysPerYear);
            }
        }

        public void AddInputs()
        {
            foreach (var input in context.Inputs)
            {
                var rate = input.Rate;

                if (rate == 0m && string.Equals(input.WageTypeCode, SwissWageTypeCodes.HourlyWage, StringComparison.Ordinal))
                {
                    rate = _employee.HourlyWage;
                }

                var amount = input.Quantity != 0m && rate != 0m ? input.Quantity * rate : input.Amount;
                AddLine(input.WageTypeCode, input.Quantity, rate, amount, input.Note);
            }

            if (IsHourly)
            {
                var hourlyTotal = _lines
                    .Where(static l => string.Equals(l.WageTypeCode, SwissWageTypeCodes.HourlyWage, StringComparison.Ordinal))
                    .Sum(static l => l.Amount);
                var percent = SwissPayrollRules.HolidayCompensationPercent(_employee.HolidayWeeks);

                if (hourlyTotal != 0m && percent > 0m)
                {
                    AddLine(SwissWageTypeCodes.HolidayCompensation, hourlyTotal, percent, hourlyTotal * percent / 100m);
                }
            }
        }

        public void AddFamilyAllowances()
        {
            var canton = (_employee.WorkCanton ?? _settings.DefaultWorkCanton).ToUpperInvariant();
            var rate = context.FamilyAllowanceRates
                .Where(r => string.Equals(r.Canton, canton, StringComparison.OrdinalIgnoreCase) && r.ValidFrom <= PeriodEnd)
                .OrderByDescending(static r => r.ValidFrom)
                .FirstOrDefault();
            var children = 0;
            var education = 0;

            foreach (var child in context.Children.Where(static c => c.AllowanceEligible))
            {
                var age = AgeAt(child.DateOfBirth, _periodStart);
                var inEducation = child.InEducation && (child.EducationEndDate is null || child.EducationEndDate >= _periodStart);

                if (child.DateOfBirth > PeriodEnd)
                {
                    continue;
                }

                if (inEducation && age is >= 15 and < 25)
                {
                    education++;
                }
                else if (age < 16)
                {
                    children++;
                }
            }

            if (children + education == 0)
            {
                return;
            }

            if (rate is null)
            {
                _warnings.Add($"Importi degli assegni familiari mancanti per il cantone {canton}.");
                return;
            }

            var share = paidDays / DaysPerMonth;
            AddCountLine(SwissWageTypeCodes.ChildAllowance, children, rate.ChildAllowance * share);
            AddCountLine(SwissWageTypeCodes.EducationAllowance, education, rate.EducationAllowance * share);
        }

        public void ComputeBases()
        {
            var contributionAge = SwissPayrollRules.IsContributionAge(_employee.DateOfBirth, context.Year);
            var pensioner = _employee.DateOfBirth is { } birth
                && SwissPayrollRules.ReferenceAgeDate(birth, _employee.Gender) < _periodStart;

            var ahvRaw = Sum(static t => t.SubjectToAhv);
            var alvRaw = Sum(static t => t.SubjectToAlv);
            var uvgRaw = Sum(static t => t.SubjectToUvg);

            _ahvBasis = contributionAge ? ahvRaw : 0m;

            if (pensioner)
            {
                _ahvBasis = Math.Max(0m, _ahvBasis - (_settings.PensionerAllowanceMonthly * paidDays / DaysPerMonth));
            }

            var yearDays = context.YearToDate.PaidDays + paidDays;
            var alvCeiling = (_settings.AlvAnnualCeiling * yearDays / DaysPerYear) - context.YearToDate.AlvBasis;
            var uvgCeiling = (_settings.UvgAnnualCeiling * yearDays / DaysPerYear) - context.YearToDate.UvgBasis;

            _alvBasis = contributionAge && !pensioner ? Clamp(alvRaw, alvCeiling) : 0m;
            _uvgBasis = Clamp(uvgRaw, uvgCeiling);
            _ktgBasis = Sum(static t => t.SubjectToKtg);
            _qstBasis = Sum(static t => t.SubjectToWithholdingTax);

            if (!pensioner && !IsShortFixedTerm())
            {
                var annual = IsHourly
                    ? _employee.HourlyWage * _employee.WeeklyHours * 52m
                    : _employee.BaseAnnualSalary;
                _bvgCoordinated = SwissPayrollRules.BvgCoordinatedSalary(annual);
            }
        }

        public void AddContributions()
        {
            var nbuRate = Rate(_employee.NonOccupationalAccidentRatePercent, _settings.NbuEmployeeRatePercent);
            var buRate = Rate(_employee.OccupationalAccidentRatePercent, _settings.BuEmployerRatePercent);
            var ktgRate = Rate(_employee.DailySicknessRatePercent, _settings.KtgEmployeeRatePercent);
            var fakRate = Rate(_employee.FamilyAllowanceFundRatePercent, _settings.FakEmployerRatePercent);
            var nbuDue = !IsHourly || _employee.WeeklyHours == 0m || _employee.WeeklyHours >= SwissPayrollRules.NbuMinimumWeeklyHours;

            _ = AddContribution(SwissWageTypeCodes.Ahv, _ahvBasis, _settings.AhvEmployeeRatePercent, employer: false);
            _ = AddContribution(SwissWageTypeCodes.Alv, _alvBasis, _settings.AlvEmployeeRatePercent, employer: false);
            _ = AddContribution(SwissWageTypeCodes.Nbu, nbuDue ? _uvgBasis : 0m, nbuRate, employer: false);
            _ = AddContribution(SwissWageTypeCodes.Ktg, _ktgBasis, ktgRate, employer: false);

            var ahvEmployer = AddContribution(SwissWageTypeCodes.AhvEmployer, _ahvBasis, _settings.AhvEmployerRatePercent, employer: true);
            var ahvTotal = ahvEmployer + (_ahvBasis * _settings.AhvEmployeeRatePercent / 100m);
            _ = AddContribution(SwissWageTypeCodes.AhvAdministration, ahvTotal, _settings.AhvAdministrationCostPercent, employer: true);
            _ = AddContribution(SwissWageTypeCodes.AlvEmployer, _alvBasis, _settings.AlvEmployerRatePercent, employer: true);
            _ = AddContribution(SwissWageTypeCodes.BuEmployer, _uvgBasis, buRate, employer: true);
            _ = AddContribution(SwissWageTypeCodes.KtgEmployer, _ktgBasis, _settings.KtgEmployerRatePercent, employer: true);
            _ = AddContribution(SwissWageTypeCodes.FakEmployer, _ahvBasis, fakRate, employer: true);

            AddPensionContributions();
        }

        public void AddWithholdingTax()
        {
            if (!_employee.IsWithholdingTaxSubject || _qstBasis <= 0m)
            {
                return;
            }

            _qstCanton = (string.Equals(_employee.ResidenceCountry ?? "CH", "CH", StringComparison.OrdinalIgnoreCase)
                ? _employee.ResidenceCanton ?? _employee.WorkCanton
                : _employee.WorkCanton) ?? _settings.DefaultWorkCanton;

            var rateIncome = WithholdingTaxRateIncome();
            var tariff = FindTariff(rateIncome);
            decimal tax;

            if (tariff is not null)
            {
                _qstRate = tariff.RatePercent;
                tax = Math.Max(_qstBasis * _qstRate / 100m, tariff.MinimumTax);
            }
            else if (_employee.WithholdingTaxRatePercent is { } manualRate)
            {
                _qstRate = manualRate;
                tax = _qstBasis * manualRate / 100m;
                _warnings.Add($"Tariffa {_employee.WithholdingTaxCode} del cantone {_qstCanton} non importata: usata l'aliquota manuale {manualRate.ToString("0.##", CultureInfo.InvariantCulture)} %.");
            }
            else
            {
                _warnings.Add($"Imposta alla fonte non calcolata: importare le tariffe ESTV del cantone {_qstCanton} (codice {_employee.WithholdingTaxCode}).");
                return;
            }

            AddLine(SwissWageTypeCodes.WithholdingTax, _qstBasis, _qstRate, -SwissPayrollRules.RoundTo5Centimes(tax));
        }

        public SwissPayrollResult Build()
        {
            var ordered = _lines.OrderBy(static l => l.SortOrder).ThenBy(static l => l.WageTypeCode, StringComparer.Ordinal).ToList();
            var gross = ordered.Where(static l => WageTypeKind.IsGross(l.Kind)).Sum(static l => l.Amount);
            var deductions = -ordered.Where(static l => string.Equals(l.Kind, WageTypeKind.Deduction, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var nonCash = ordered.Where(static l => string.Equals(l.Kind, WageTypeKind.NonCash, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var expenses = ordered.Where(static l => string.Equals(l.Kind, WageTypeKind.Expense, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var employer = ordered.Where(static l => string.Equals(l.Kind, WageTypeKind.Employer, StringComparison.Ordinal)).Sum(static l => l.Amount);
            var net = gross - deductions;

            Payslip payslip = new()
            {
                EmployeeId = _employee.Id,
                Year = context.Year,
                Month = context.Month,
                EmployeeName = _employee.FullName,
                PaidDays = paidDays,
                GrossSalary = Round(gross),
                AhvBasis = Round(_ahvBasis),
                AlvBasis = Round(_alvBasis),
                UvgBasis = Round(_uvgBasis),
                KtgBasis = Round(_ktgBasis),
                BvgInsuredSalary = Round(_bvgCoordinated),
                WithholdingTaxBasis = Round(_employee.IsWithholdingTaxSubject ? _qstBasis : 0m),
                WithholdingTaxRatePercent = _qstRate,
                WithholdingTaxCode = _employee.IsWithholdingTaxSubject ? _employee.WithholdingTaxCode : null,
                WithholdingTaxCanton = _qstCanton,
                EmployeeDeductions = Round(deductions),
                NetSalary = Round(net),
                PayoutAmount = SwissPayrollRules.RoundTo5Centimes(net - nonCash + expenses),
                EmployerContributions = Round(employer),
                Warnings = _warnings.Count == 0 ? null : string.Join(Environment.NewLine, _warnings),
            };

            return new SwissPayrollResult(payslip, ordered);
        }

        private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        private static decimal Clamp(decimal value, decimal ceiling) => Math.Max(0m, Math.Min(value, ceiling));

        private static decimal Rate(decimal employeeRate, decimal companyRate) => employeeRate > 0m ? employeeRate : companyRate;

        private static int AgeAt(DateTime birth, DateTime date)
        {
            var age = date.Year - birth.Year;
            return birth.AddYears(age) > date ? age - 1 : age;
        }

        private bool IsShortFixedTerm() => _employee.IsFixedTermContract
            && _employee.TerminationDate is { } end
            && end <= _employee.HireDate.AddMonths(3);

        private void AddPensionContributions()
        {
            var employerShare = Math.Clamp(_settings.BvgEmployerSharePercent, 50m, 100m);
            decimal employeeContribution;
            decimal employerContribution;

            if (_employee.PensionFundEmployeeRatePercent is { } planRate && planRate > 0m)
            {
                var basis = Sum(static t => t.SubjectToBvg);
                employeeContribution = basis * planRate / 100m;
                employerContribution = employerShare >= 100m ? 0m : employeeContribution * employerShare / (100m - employerShare);
            }
            else
            {
                if (_bvgCoordinated <= 0m || !SwissPayrollRules.IsContributionAge(_employee.DateOfBirth, context.Year))
                {
                    return;
                }

                var percent = SwissPayrollRules.BvgAgeCreditPercent(_employee.DateOfBirth, context.Year) + _settings.BvgRiskRatePercent;
                var total = _bvgCoordinated * percent / 100m / 12m * paidDays / DaysPerMonth;
                employeeContribution = total * (100m - employerShare) / 100m;
                employerContribution = total - employeeContribution;
            }

            AddLine(SwissWageTypeCodes.Bvg, _bvgCoordinated, 0m, -SwissPayrollRules.RoundTo5Centimes(employeeContribution));
            AddLine(SwissWageTypeCodes.BvgEmployer, _bvgCoordinated, 0m, SwissPayrollRules.RoundTo5Centimes(employerContribution));
        }

        private decimal WithholdingTaxRateIncome()
        {
            if (SwissPayrollRules.UsesAnnualWithholdingTaxModel(_qstCanton))
            {
                // Annual model: the rate is determined on the regular monthly income including the pro-rata share of the 13th salary.
                var regular = _lines
                    .Where(l => context.WageTypes.TryGetValue(l.WageTypeCode, out var t) && t.SubjectToWithholdingTax && !t.IsIrregular
                        && !string.Equals(l.WageTypeCode, SwissWageTypeCodes.ThirteenthSalary, StringComparison.Ordinal))
                    .Sum(static l => l.Amount);
                var installments = Math.Max(_employee.SalaryInstallmentsPerYear, 12);
                return regular * installments / 12m * DaysPerMonth / paidDays;
            }

            // Monthly model: the income of an incomplete month is converted to a full month to determine the rate.
            return _qstBasis * DaysPerMonth / paidDays;
        }

        private WithholdingTaxTariff? FindTariff(decimal rateIncome)
        {
            var code = (_employee.WithholdingTaxCode ?? string.Empty).Trim().ToUpperInvariant();
            var rows = context.WithholdingTaxTariffs
                .Where(t => string.Equals(t.Canton, _qstCanton, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(t.TariffCode, code, StringComparison.OrdinalIgnoreCase)
                    && t.ValidFrom <= PeriodEnd)
                .ToList();

            if (rows.Count == 0)
            {
                return null;
            }

            var validFrom = rows.Max(static t => t.ValidFrom);
            var current = rows.Where(t => t.ValidFrom == validFrom).OrderBy(static t => t.IncomeFrom).ToList();
            return current.LastOrDefault(t => t.IncomeFrom <= rateIncome) ?? current[0];
        }

        private decimal Sum(Func<PayrollWageType, bool> subject) => _lines
            .Where(l => context.WageTypes.TryGetValue(l.WageTypeCode, out var type)
                && WageTypeKind.IsGross(type.Kind)
                && subject(type))
            .Sum(static l => l.Amount);

        private decimal AddContribution(string code, decimal basis, decimal ratePercent, bool employer)
        {
            if (basis <= 0m || ratePercent <= 0m)
            {
                return 0m;
            }

            var amount = SwissPayrollRules.RoundTo5Centimes(basis * ratePercent / 100m);
            AddLine(code, basis, ratePercent, employer ? amount : -amount);
            return amount;
        }

        private void AddCountLine(string code, int count, decimal amountEach)
        {
            if (count > 0)
            {
                AddLine(code, count, amountEach, count * amountEach);
            }
        }

        private void AddLine(string code, decimal quantity, decimal rate, decimal amount, string? note = null)
        {
            if (!context.WageTypes.TryGetValue(code, out var type))
            {
                _warnings.Add($"Genere salariale {code} non definito: riga ignorata.");
                return;
            }

            var description = string.IsNullOrWhiteSpace(note) ? type.Name : $"{type.Name} - {note}";
            _lines.Add(new PayslipLine
            {
                WageTypeCode = code,
                Description = description,
                Kind = type.Kind,
                Quantity = Math.Round(quantity, 4, MidpointRounding.AwayFromZero),
                Rate = Math.Round(rate, 4, MidpointRounding.AwayFromZero),
                Amount = Round(amount),
                CertificateBox = type.CertificateBox,
                SortOrder = type.SortOrder,
            });
        }
    }
}
