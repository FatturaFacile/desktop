// <copyright file="ApplicationServiceRegistration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Banking;
using FatturaFacile.Application.Common.Behaviors;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Application.Features.Reports.Builders;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Application.Strategies;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application;

/// <summary>
/// Service collection extensions registering Application layer dependencies.
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Registers MediatR CQRS handlers, AutoMapper profiles, FluentValidation behaviors, and localized country strategies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceRegistration).Assembly;

        _ = services.AddAutoMapper(cfg => cfg.AddMaps(assembly));
        _ = services.AddValidatorsFromAssembly(assembly);

        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly).AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)).AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));

        _ = services.AddSingleton<ICountryStrategyFactory, CountryStrategyFactory>();

        _ = services.AddTransient<IReportDataBuilder, InvoiceReportDataBuilder>();
        _ = services.AddTransient<IReportDataBuilder, PayslipReportDataBuilder>();
        _ = services.AddTransient<IReportDataBuilder, SalaryCertificateReportDataBuilder>();
        _ = services.AddTransient<IReportDataBuilder, TimeSheetReportDataBuilder>();
        _ = services.AddTransient<WorkingTimeCalculator>();
        _ = services.AddTransient<TimeAccess>();
        _ = services.AddTransient<JournalWriter>();
        _ = services.AddTransient<BankReconciler>();
        _ = services.AddTransient<PurchaseInvoiceImporter>();

        AddRecordHandlers<Contact>(services);
        AddRecordHandlers<Product>(services);
        AddRecordHandlers<ChartOfAccount>(services);
        AddRecordHandlers<BankAccount>(services);
        AddRecordHandlers<BankTransaction>(services);
        AddRecordHandlers<PurchaseInvoice>(services);
        AddRecordHandlers<PurchaseInvoiceLine>(services);
        AddRecordHandlers<VatReturn>(services);
        AddRecordHandlers<FixedAsset>(services);
        AddRecordHandlers<FixedAssetDepreciation>(services);
        AddRecordHandlers<GeneralLedgerEntry>(services);
        AddRecordHandlers<GeneralLedgerLine>(services);

        AddPayrollRecordHandlers<HrEmployee>(services, writable: true);
        AddPayrollRecordHandlers<HrEmployeeChild>(services, writable: true);
        AddPayrollRecordHandlers<HrAbsence>(services, writable: true);
        AddPayrollRecordHandlers<PayrollInput>(services, writable: true);
        AddPayrollRecordHandlers<PayrollWageType>(services, writable: true);
        AddPayrollRecordHandlers<FamilyAllowanceRate>(services, writable: true);
        AddPayrollRecordHandlers<SwissPayrollSettings>(services, writable: true);
        AddPayrollRecordHandlers<PublicHoliday>(services, writable: true);
        AddPayrollRecordHandlers<PayrollRun>(services, writable: false);
        AddPayrollRecordHandlers<Payslip>(services, writable: false);
        AddPayrollRecordHandlers<PayslipLine>(services, writable: false);
        AddPayrollRecordHandlers<SalaryCertificate>(services, writable: false);

        return services;
    }

    /// <summary>
    /// Registers the closed generic handlers of the master data record requests for an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    private static void AddRecordHandlers<TEntity>(IServiceCollection services)
        where TEntity : class, IBaseEntity, new()
    {
        _ = services.AddTransient<IRequestHandler<GetRecordsQuery<TEntity>, IReadOnlyList<TEntity>>, GetRecordsQueryHandler<TEntity>>();
        _ = services.AddTransient<IRequestHandler<SaveRecordCommand<TEntity>, Guid>, SaveRecordCommandHandler<TEntity>>();
        _ = services.AddTransient<IRequestHandler<DeleteRecordCommand<TEntity>, bool>, DeleteRecordCommandHandler<TEntity>>();
    }

    /// <summary>
    /// Registers the closed generic handlers of the payroll master data requests for an entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="writable">Whether save and delete are allowed.</param>
    private static void AddPayrollRecordHandlers<TEntity>(IServiceCollection services, bool writable)
        where TEntity : class, IBaseEntity, new()
    {
        _ = services.AddTransient<IRequestHandler<GetPayrollRecordsQuery<TEntity>, IReadOnlyList<TEntity>>, GetPayrollRecordsQueryHandler<TEntity>>();

        if (writable)
        {
            _ = services.AddTransient<IRequestHandler<SavePayrollRecordCommand<TEntity>, Guid>, SavePayrollRecordCommandHandler<TEntity>>();
            _ = services.AddTransient<IRequestHandler<DeletePayrollRecordCommand<TEntity>, bool>, DeletePayrollRecordCommandHandler<TEntity>>();
        }
    }
}
