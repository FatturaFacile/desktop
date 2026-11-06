// <copyright file="ContactsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Customers and suppliers: list, creation and editing with structured address, VAT/UID and Italian e-invoicing data.
/// </summary>
internal sealed partial class ContactsViewModel : PayrollSectionViewModel
{
    [ObservableProperty]
    private Contact? _selectedContact;

    [ObservableProperty]
    private Contact _editing = NewContact();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactsViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender instance.</param>
    /// <param name="currentTenantService">The current tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    public ContactsViewModel(ISender sender, ICurrentTenantService currentTenantService, ILocalizationService localizationService)
        : base(sender, localizationService)
    {
        ArgumentNullException.ThrowIfNull(currentTenantService);
        currentTenantService.CurrentTenantChanged += (_, _) => _ = LoadAsync();
    }

    /// <summary>
    /// Gets the records.
    /// </summary>
    public ObservableCollection<Contact> Contacts { get; } = [];

    /// <summary>
    /// Loads the records (used by the shell navigation).
    /// </summary>
    /// <returns>A task representing the load.</returns>
    public Task LoadContactsAsync() => LoadAsync();

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedContact?.Id;
        var records = await Sender.Send(new GetRecordsQuery<Contact>());
        Contacts.Clear();

        foreach (var record in records.OrderBy(static r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase))
        {
            Contacts.Add(record);
        }

        SelectedContact = Contacts.FirstOrDefault(r => r.Id == selectedId) ?? Contacts.FirstOrDefault();
    });

    private static Contact NewContact() => new() { Id = Guid.Empty, Type = "client", Country = "CH" };

    partial void OnSelectedContactChanged(Contact? value) => Editing = value is null ? NewContact() : RecordCopier.Clone(value);

    [RelayCommand]
    private void New()
    {
        SelectedContact = null;
        Editing = NewContact();
    }

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SaveRecordCommand<Contact>(Editing));
            await LoadAsync();
            SelectedContact = Contacts.FirstOrDefault(r => r.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteContactAsync() => SelectedContact is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteRecordCommand<Contact>(SelectedContact.Id));
                await LoadAsync();
            },
            "Common_Deleted");
}
