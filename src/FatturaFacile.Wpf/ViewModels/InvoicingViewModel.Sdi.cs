// <copyright file="InvoicingViewModel.Sdi.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Invoicing.Sdi;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Transmission to the Sistema di Interscambio.
/// </content>
internal sealed partial class InvoicingViewModel
{
    /// <summary>
    /// Sends the selected invoice to the SDI.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task SendToSdiAsync()
    {
        if (SelectedInvoice is null)
        {
            StatusMessage = _localizationService["Invoicing_NoSelection"];
            return;
        }

        IsLoading = true;

        try
        {
            var channel = await _sender.Send(new SendInvoiceToSdiCommand(SelectedInvoice.Id));
            await LoadInvoicesAsync();
            StatusMessage = string.Format(_localizationService.CurrentCulture, _localizationService["Invoicing_SdiSent"], channel);
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reads the receipts of the SDI and the received supplier invoices.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task ProcessSdiInboxAsync()
    {
        IsLoading = true;

        try
        {
            var result = await _sender.Send(new ProcessSdiInboxCommand());
            await LoadInvoicesAsync();
            StatusMessage = string.Format(
                _localizationService.CurrentCulture,
                _localizationService["Invoicing_SdiInboxDone"],
                result.Notifications,
                result.PurchaseInvoices,
                string.Join(' ', result.Messages));
        }
        catch (Exception ex)
        {
            StatusMessage = $"{_localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
