// <copyright file="PayrollSectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Payroll;

/// <summary>
/// Base class of the payroll sections: MediatR sender, localization, busy flag and status message.
/// </summary>
internal abstract partial class PayrollSectionViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrollSectionViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="localizationService">The localization service.</param>
    protected PayrollSectionViewModel(ISender sender, ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(localizationService);

        Sender = sender;
        Localization = localizationService;
    }

    /// <summary>
    /// Gets the MediatR sender.
    /// </summary>
    protected ISender Sender { get; }

    /// <summary>
    /// Gets the localization service.
    /// </summary>
    protected ILocalizationService Localization { get; }

    /// <summary>
    /// Loads the data of the section.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public abstract Task LoadAsync();

    /// <summary>
    /// Runs an operation showing the busy state and reporting errors in the status message.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="successKey">The localization key of the success message, if any.</param>
    /// <returns>A task representing the operation.</returns>
    protected Task RunAsync(Func<Task> operation, string? successKey = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return RunInternalAsync(operation, successKey);
    }

    private async Task RunInternalAsync(Func<Task> operation, string? successKey)
    {
        IsBusy = true;

        try
        {
            await operation();

            if (successKey is not null)
            {
                StatusMessage = Localization[successKey];
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
