// <copyright file="CustomerLifecycleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Features.Contacts.Queries;
using FatturaFacile.Application.Features.CustomerLifecycle;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Drives the customer lifecycle view (issue #193): health and post-sales KPIs of a customer and the timeline of every touchpoint
/// (activities, opportunities, invoices, tickets, field interventions, contracts). It reads the web-api, so it needs a server session.
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localization">The localization service.</param>
internal sealed partial class CustomerLifecycleViewModel(ISender sender, ILocalizationService localization) : ObservableObject
{
    private const string NoValue = "-";
    private const string DateTimeFormat = "g";

    [ObservableProperty]
    private CustomerChoice? _selectedCustomer;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _healthText = NoValue;

    [ObservableProperty]
    private string _churnText = NoValue;

    [ObservableProperty]
    private string _lifetimeValueText = NoValue;

    [ObservableProperty]
    private string _pipelineText = NoValue;

    [ObservableProperty]
    private string _overdueText = NoValue;

    [ObservableProperty]
    private string _openTicketsText = NoValue;

    [ObservableProperty]
    private string _slaBreachText = NoValue;

    [ObservableProperty]
    private string _mttrText = NoValue;

    [ObservableProperty]
    private string _contractsText = NoValue;

    [ObservableProperty]
    private string _coverageText = NoValue;

    [ObservableProperty]
    private string _hoursRemainingText = NoValue;

    [ObservableProperty]
    private string _interventionsOpenText = NoValue;

    [ObservableProperty]
    private string _interventionsDoneText = NoValue;

    /// <summary>
    /// Gets the customers of the picker.
    /// </summary>
    internal ObservableCollection<CustomerChoice> Customers { get; } = [];

    /// <summary>
    /// Gets the timeline of the selected customer, newest first.
    /// </summary>
    internal ObservableCollection<CustomerTimelineRow> Timeline { get; } = [];

    /// <summary>
    /// Loads the customer picker.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var contacts = await sender.Send(new GetContactsQuery());

            Customers.Clear();
            foreach (var contact in contacts)
            {
                Customers.Add(new CustomerChoice(contact.Id, Label(contact.CompanyName, contact.FirstName, contact.LastName)));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Fetches the summary and the timeline of the selected customer from the server.
    /// </summary>
    /// <returns>A task representing the fetch.</returns>
    [RelayCommand]
    public async Task ShowAsync()
    {
        if (SelectedCustomer is not { } customer)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var view = await sender.Send(new GetCustomerLifecycleQuery(customer.Id, []));
            if (view is null)
            {
                Clear();
                StatusMessage = localization["CustomerLifecycle_NoAccount"];
                return;
            }

            Apply(view);
        }
        catch (HttpRequestException)
        {
            Clear();
            StatusMessage = localization["CustomerLifecycle_LoadFailed"];
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string Label(string? company, string? first, string? last)
    {
        if (!string.IsNullOrWhiteSpace(company))
        {
            return company;
        }

        var person = ((first ?? string.Empty) + " " + (last ?? string.Empty)).Trim();

        return person.Length > 0 ? person : NoValue;
    }

    private static string Number(double value) => value.ToString("N2", CultureInfo.CurrentCulture);

    private void Apply(CustomerLifecycleView view)
    {
        var kpis = view.Summary.Kpis;
        var culture = CultureInfo.CurrentCulture;

        HealthText = Number(view.Summary.Health.HealthIndex);
        ChurnText = view.Summary.Health.ChurnRisk ?? NoValue;
        LifetimeValueText = kpis.LifetimeValue;
        PipelineText = kpis.OpenPipelineValue;
        OverdueText = kpis.OverdueReceivableAmount;
        OpenTicketsText = kpis.OpenTickets.ToString(culture);
        SlaBreachText = Number(kpis.SlaBreachRate) + " %";
        MttrText = kpis.MttrHours is { } hours ? Number(hours) : NoValue;
        ContractsText = kpis.ActiveContracts.ToString(culture);
        CoverageText = localization["CustomerLifecycle_Coverage_" + kpis.ContractCoverage];
        HoursRemainingText = kpis.ContractHoursRemaining;
        InterventionsOpenText = kpis.InterventionsScheduled.ToString(culture);
        InterventionsDoneText = kpis.InterventionsCompleted.ToString(culture);

        Timeline.Clear();
        foreach (var item in view.Timeline)
        {
            Timeline.Add(new CustomerTimelineRow(
                item.OccurredAt.ToLocalTime().ToString(DateTimeFormat, culture),
                localization["CustomerLifecycle_Type_" + item.Type],
                item.Title,
                item.Status ?? string.Empty,
                item.Amount ?? string.Empty));
        }

        HasData = true;
    }

    private void Clear()
    {
        Timeline.Clear();
        HasData = false;
    }
}
