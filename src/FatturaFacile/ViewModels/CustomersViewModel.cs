// <copyright file="CustomersViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Contacts;
using FatturaFacile.ViewModels;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model governing the customer directory view, presenting customer profiles, search, and selection.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CustomersViewModel"/> class.
/// </remarks>
/// <param name="mediator">The MediatR sender instance.</param>
public partial class CustomersViewModel(ISender mediator) : ViewModelBase
{
    private readonly ISender _mediator = mediator;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;


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
