// <copyright file="PayrollSettingsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Payroll.Commands;
using FatturaFacile.Application.Features.Payroll.Queries;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Swiss payroll settings of the company (rates, ceilings, pension fund, insurers).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PayrollSettingsViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class PayrollSettingsViewModel(ISender sender, ILocalizationService localizationService) : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private SwissPayrollSettings _settings = new();

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var settings = await Sender.Send(new GetPayrollRecordsQuery<SwissPayrollSettings>());
        Settings = settings.FirstOrDefault() ?? new SwissPayrollSettings { Id = Guid.Empty };
    });

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SavePayrollRecordCommand<SwissPayrollSettings>(Settings));
            await LoadAsync();
        },
        "Common_Saved");
}
