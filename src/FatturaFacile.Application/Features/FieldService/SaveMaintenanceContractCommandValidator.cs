// <copyright file="SaveMaintenanceContractCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using FluentValidation;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Validator for <see cref="SaveMaintenanceContractCommand"/>: the rules of the web-api request, so a contract the server would
/// refuse is caught before the call. The messages are localization keys, resolved by the view.
/// </summary>
public sealed class SaveMaintenanceContractCommandValidator : AbstractValidator<SaveMaintenanceContractCommand>
{
    private const int MaxNumber = 60;
    private const int MaxTitle = 180;
    private const int CurrencyLength = 3;
    private const int MaxCalendarEvery = 120;
    private const int MaxLeadDays = 365;
    private const decimal MaxLeadPercent = 50m;

    private static readonly string[] Units = ["day", "week", "month", "year"];

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveMaintenanceContractCommandValidator"/> class.
    /// </summary>
    public SaveMaintenanceContractCommandValidator()
    {
        _ = RuleFor(x => x.Contract.CustomerId).Must(id => Guid.TryParse(id, out var parsed) && parsed != Guid.Empty).WithMessage("FieldService_Err_ContractCustomer");
        _ = RuleFor(x => x.Contract.Number).Must(value => HasTextUpTo(value, MaxNumber)).WithMessage("FieldService_Err_ContractNumber");
        _ = RuleFor(x => x.Contract.Title).Must(value => HasTextUpTo(value, MaxTitle)).WithMessage("FieldService_Err_ContractTitle");
        _ = RuleFor(x => x.Contract.Currency).Length(CurrencyLength).WithMessage("FieldService_Err_Currency");
        _ = RuleFor(x => x.Contract).Must(HaveConsistentDates).WithMessage("FieldService_Err_ContractDates");
        _ = RuleFor(x => x.Contract).Must(HaveValidAmounts).WithMessage("FieldService_Err_ContractAmounts");
        _ = RuleFor(x => x.Contract.Frequency).Must(BeValidFrequency).WithMessage("FieldService_Err_ContractFrequency");
    }

    private static bool HasTextUpTo(string value, int maxLength) => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength;

    private static bool HaveConsistentDates(FieldServiceContractInput contract) => contract.EndDate is null || contract.EndDate >= contract.StartDate;

    private static bool HaveValidAmounts(FieldServiceContractInput contract) =>
        contract.CalloutFee >= 0 && (contract.HourlyRate is null or >= 0) && (contract.IncludedVisits is null or >= 0);

    private static bool BeValidFrequency(PreventiveFrequency? rule) =>
        rule is null
        || (CalendarIsValid(rule) && MeterIsValid(rule) && (rule.LeadDays is null or (>= 0 and <= MaxLeadDays)));

    private static bool CalendarIsValid(PreventiveFrequency rule) =>
        (rule.CalendarEvery is null && rule.CalendarUnit is null) || HasValidCalendar(rule);

    private static bool HasValidCalendar(PreventiveFrequency rule) =>
        rule.CalendarEvery is >= 1 and <= MaxCalendarEvery && rule.CalendarUnit is not null && Units.Contains(rule.CalendarUnit);

    private static bool MeterIsValid(PreventiveFrequency rule) =>
        (rule.OperatingHours is null or > 0) && (rule.Cycles is null or > 0) && (rule.LeadPercent is null or (>= 0 and <= MaxLeadPercent));
}
