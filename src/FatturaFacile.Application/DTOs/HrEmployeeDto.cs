// <copyright file="HrEmployeeDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer object for an employee and compensation profile.
/// </summary>
public sealed class HrEmployeeDto
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the mandatory owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the employee number.
    /// </summary>
    public string EmployeeNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the social security number / AVS / CF.
    /// </summary>
    public string? SocialSecurityNumber { get; set; }

    /// <summary>
    /// Gets or sets the department name.
    /// </summary>
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the employment status ("active", "onboarding", "suspended", "terminated").
    /// </summary>
    public string EmploymentStatus { get; set; } = "active";

    /// <summary>
    /// Gets or sets the employment type ("full_time", "part_time", "contractor", "hourly").
    /// </summary>
    public string EmploymentType { get; set; } = "full_time";

    /// <summary>
    /// Gets or sets the hire date.
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Gets or sets the annual gross base salary (RAL).
    /// </summary>
    public decimal BaseAnnualSalary { get; set; }

    /// <summary>
    /// Gets or sets the standard hourly cost rate.
    /// </summary>
    public decimal HourlyCostRate { get; set; }

    /// <summary>
    /// Gets or sets the salary currency ("CHF", "EUR").
    /// </summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>
    /// Gets or sets the FTE workload ratio (e.g. 1.0 = 100%).
    /// </summary>
    public decimal FteRatio { get; set; } = 1.0m;

    /// <summary>
    /// Gets or sets the work schedule template code.
    /// </summary>
    public string WorkScheduleCode { get; set; } = "STD-40H";

    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the number of salary installments per year.
    /// </summary>
    public int SalaryInstallmentsPerYear { get; set; } = 12;

    /// <summary>
    /// Gets or sets a value indicating whether the contract is fixed-term.
    /// </summary>
    public bool IsFixedTermContract { get; set; }

    /// <summary>
    /// Gets or sets the Swiss canton of work.
    /// </summary>
    public string? WorkCanton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Swiss withholding tax applies.
    /// </summary>
    public bool IsWithholdingTaxSubject { get; set; }

    /// <summary>
    /// Gets or sets the cantonal withholding tax tariff code.
    /// </summary>
    public string? WithholdingTaxCode { get; set; }

    /// <summary>
    /// Gets or sets the withholding tax rate in percent.
    /// </summary>
    public decimal? WithholdingTaxRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the employee non-occupational accident insurance rate in percent.
    /// </summary>
    public decimal NonOccupationalAccidentRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the employer occupational accident insurance rate in percent.
    /// </summary>
    public decimal OccupationalAccidentRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the employee daily sickness allowance insurance rate in percent.
    /// </summary>
    public decimal DailySicknessRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the employer family compensation fund rate in percent.
    /// </summary>
    public decimal FamilyAllowanceFundRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the optional pension fund employee rate in percent.
    /// </summary>
    public decimal? PensionFundEmployeeRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the Italian regional IRPEF surcharge rate in percent.
    /// </summary>
    public decimal RegionalSurchargeRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the Italian municipal IRPEF surcharge rate in percent.
    /// </summary>
    public decimal MunicipalSurchargeRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the formatted full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gender ("M" or "F"), for the AHV reference age.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the nationality (ISO country code).
    /// </summary>
    public string? Nationality { get; set; }

    /// <summary>
    /// Gets or sets the residence permit: CH (Swiss), C, B, L, G (cross-border), F, N, S or Ci.
    /// </summary>
    public string? ResidencePermit { get; set; }

    /// <summary>
    /// Gets or sets the country of residence (ISO code).
    /// </summary>
    public string? ResidenceCountry { get; set; } = "CH";

    /// <summary>
    /// Gets or sets the canton of residence (withholding tax canton for residents).
    /// </summary>
    public string? ResidenceCanton { get; set; }

    /// <summary>
    /// Gets or sets the street of the home address.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Gets or sets the house number.
    /// </summary>
    public string? HouseNumber { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets the civil status (single, married, divorced, widowed, registered partnership).
    /// </summary>
    public string? CivilStatus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether church tax is due (withholding tax tariffs ending with "Y").
    /// </summary>
    public bool ChurchTax { get; set; }

    /// <summary>
    /// Gets or sets the pay type: "monthly" or "hourly".
    /// </summary>
    public string PayType { get; set; } = "monthly";

    /// <summary>
    /// Gets or sets the hourly wage (hourly paid employees).
    /// </summary>
    public decimal HourlyWage { get; set; }

    /// <summary>
    /// Gets or sets the contractual weekly hours.
    /// </summary>
    public decimal WeeklyHours { get; set; } = 42m;

    /// <summary>
    /// Gets or sets the holiday weeks per year (holiday compensation of hourly paid employees).
    /// </summary>
    public int HolidayWeeks { get; set; } = 4;

    /// <summary>
    /// Gets or sets the IBAN for the salary payment.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    /// Gets or sets the working time recording mode: Detailed (art. 73 OLL 1), Simplified (art. 73b) or Waived (art. 73a).
    /// </summary>
    public string TimeRecordingMode { get; set; } = "Detailed";

    /// <summary>
    /// Gets or sets the legal maximum weekly hours (45 for industry, offices, technical staff and large retail; 50 otherwise).
    /// </summary>
    public decimal LegalMaxWeeklyHours { get; set; } = 45m;

    /// <summary>
    /// Gets or sets the working days per week (target time).
    /// </summary>
    public int WorkDaysPerWeek { get; set; } = 5;

    /// <summary>
    /// Gets or sets the date of the agreement allowing simplified recording or the waiver.
    /// </summary>
    public DateTime? TimeRecordingAgreementDate { get; set; }

    /// <summary>
    /// Gets or sets the user account linked to the employee (self-service clocking).
    /// </summary>
    public Guid? UserId { get; set; }
}
