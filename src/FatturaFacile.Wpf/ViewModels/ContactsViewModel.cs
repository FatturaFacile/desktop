// <copyright file="ContactsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Contacts.Commands;
using FatturaFacile.Application.Features.Contacts.Queries;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing CRM, customers, and suppliers directory.
/// </summary>
public sealed partial class ContactsViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ObservableCollection<ContactDto> _contacts = [];

    [ObservableProperty]
    private ContactDto? _selectedContact;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public ContactsViewModel(
        ISender sender,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(currentTenantService);
        ArgumentNullException.ThrowIfNull(localizationService);

        _sender = sender;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;
        _statusMessage = localizationService["Common_Ready"];

        _currentTenantService.CurrentTenantChanged += (_, _) => _ = OnTenantChangedAsync();
    }

    /// <summary>
    /// Loads all contacts for current tenant.
    /// </summary>
    /// <returns>A task representing the load operation.</returns>
    [RelayCommand]
    public async Task LoadContactsAsync()
    {
        IsLoading = true;
        StatusMessage = _localizationService["Common_Loading"];
        try
        {
            var contacts = await _sender.Send(new GetContactsQuery());
            Contacts.Clear();
            foreach (var contact in contacts)
            {
                Contacts.Add(contact);
            }

            SelectedContact = Contacts.FirstOrDefault();
            StatusMessage = $"{Contacts.Count} {_localizationService["Nav_Contacts"]}.";
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
    /// Deletes the selected contact.
    /// </summary>
    /// <returns>A task representing contact deletion.</returns>
    [RelayCommand]
    public async Task DeleteContactAsync()
    {
        if (SelectedContact == null)
        {
            StatusMessage = _localizationService["Common_Ready"];
            return;
        }

        IsLoading = true;
        try
        {
            _ = await _sender.Send(new DeleteContactCommand(SelectedContact.Id));
            _ = Contacts.Remove(SelectedContact);
            SelectedContact = Contacts.FirstOrDefault();
            StatusMessage = _localizationService["Common_Success"];
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

    private Task OnTenantChangedAsync()
    {
        SelectedContact = null;
        Contacts.Clear();
        return LoadContactsAsync();
    }
}
