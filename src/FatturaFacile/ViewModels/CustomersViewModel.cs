// <copyright file="CustomersViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Contacts;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing the customer directory view, presenting customer profiles, search, and selection.
/// </summary>
public partial class CustomersViewModel : ViewModelBase
{
    private readonly ISender _mediator;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersViewModel"/> class.
    /// </summary>
    /// <param name="mediator">The MediatR sender instance.</param>
    public CustomersViewModel(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _mediator.Send(new GetCustomersQuery());
            Customers.Clear();
            foreach (var c in list) Customers.Add(c);
            SelectedCustomer = Customers.FirstOrDefault();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
