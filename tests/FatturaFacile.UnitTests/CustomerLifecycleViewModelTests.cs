// <copyright file="CustomerLifecycleViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Application.Features.CustomerLifecycle;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the customer lifecycle view model.
/// </summary>
public sealed class CustomerLifecycleViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    /// <summary>
    /// A customer without a CRM account shows the localized notice and no data.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Show_NoAccount_ShowsNotice()
    {
        var (viewModel, _) = Create(view: null, failure: null);

        await viewModel.ShowCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasData);
        Assert.Equal("[CustomerLifecycle_NoAccount]", viewModel.StatusMessage);
    }

    /// <summary>
    /// The KPIs and the timeline of the account are shown with the localized touchpoint type.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Show_WithAccount_FillsKpisAndTimeline()
    {
        CustomerLifecycleSummary summary = new(new CustomerLifecycleHealth(70, "medium"), new CustomerLifecycleKpis("1000.00", "500.00", 1, "0.00", 2, 10, 4.5, 1, "covered", "8.00", 0, 1, 3));
        List<CustomerTimelineEvent> timeline = [new CustomerTimelineEvent("ticket", "t1", DateTimeOffset.UtcNow, "Chiller offline", "open", null, "T-1")];
        var (viewModel, _) = Create(new CustomerLifecycleView(summary, timeline), failure: null);

        await viewModel.ShowCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasData);
        Assert.Equal("1000.00", viewModel.LifetimeValueText);
        Assert.Equal("[CustomerLifecycle_Coverage_covered]", viewModel.CoverageText);
        var row = Assert.Single(viewModel.Timeline);
        Assert.Equal("[CustomerLifecycle_Type_ticket]", row.TypeText);
        Assert.Equal("Chiller offline", row.Title);
    }

    /// <summary>
    /// A server failure clears the data and shows the localized failure notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Show_ServerFailure_ShowsFailure()
    {
        var (viewModel, _) = Create(view: null, new HttpRequestException("boom"));

        await viewModel.ShowCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasData);
        Assert.Equal("[CustomerLifecycle_LoadFailed]", viewModel.StatusMessage);
    }

    private static (CustomerLifecycleViewModel ViewModel, Mock<ISender> Sender) Create(CustomerLifecycleView? view, Exception? failure)
    {
        Mock<ISender> sender = new();
        var setup = sender.Setup(s => s.Send(It.IsAny<GetCustomerLifecycleQuery>(), Any));
        _ = failure is null ? setup.ReturnsAsync(view) : setup.ThrowsAsync(failure);

        Mock<ILocalizationService> localization = new();
        _ = localization.Setup(l => l[It.IsAny<string>()]).Returns((string key) => "[" + key + "]");

        CustomerLifecycleViewModel viewModel = new(sender.Object, localization.Object)
        {
            SelectedCustomer = new CustomerChoice(Guid.NewGuid(), "Rossi SA"),
        };

        return (viewModel, sender);
    }
}
