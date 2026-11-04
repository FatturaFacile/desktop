// <copyright file="DesktopApplicationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Desktop.Wpf.ViewModels;
using FatturaFacile.Identity.Application.DTOs;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FatturaFacile.Desktop.Tests;

/// <summary>
/// Unit tests verifying desktop presentation services, localizer switching, and view model workflows.
/// </summary>
public class DesktopApplicationTests
{
    /// <summary>
    /// Verifies that desktop localizer translates strings and switches active cultures reactively.
    /// </summary>
    [Fact]
    public void Localizer_ShouldTranslateAndSwitchCulturesDynamically()
    {
        // Arrange
        var localizer = new DesktopLocalizer();

        // Act & Assert - Default EN
        localizer.SetCulture("en");
        _ = localizer["Nav.Invoices"].Should().Be("Invoices");
        _ = localizer["Dashboard.TotalSales"].Should().Be("Total Revenue");

        // Act & Assert - Italian
        localizer.SetCulture("it");
        _ = localizer["Nav.Invoices"].Should().Be("Fatture");
        _ = localizer["Dashboard.TotalSales"].Should().Be("Fatturato Totale");

        // Act & Assert - German
        localizer.SetCulture("de");
        _ = localizer["Nav.Invoices"].Should().Be("Rechnungen");
        _ = localizer["Dashboard.TotalSales"].Should().Be("Gesamtumsatz");

        // Act & Assert - French
        localizer.SetCulture("fr");
        _ = localizer["Nav.Invoices"].Should().Be("Factures");
        _ = localizer["Dashboard.TotalSales"].Should().Be("Chiffre d'Affaires Total");
    }

    /// <summary>
    /// Verifies that navigation service activates target view models and raises notification events.
    /// </summary>
    [Fact]
    public void NavigationService_ShouldTransitionViewModelsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var localizer = new DesktopLocalizer();
        var userContext = new DesktopUserContext();
        var tenantContext = new DesktopTenantContext();

        _ = services.AddSingleton<IDesktopLocalizer>(localizer);
        _ = services.AddSingleton(userContext);
        _ = services.AddSingleton(tenantContext);
        _ = services.AddSingleton<INavigationService, NavigationService>();
        _ = services.AddSingleton<DashboardViewModel>();

        using var provider = services.BuildServiceProvider();
        var navService = provider.GetRequiredService<INavigationService>();

        ViewModelBase? navigatedViewModel = null;
        navService.CurrentViewModelChanged += vm => navigatedViewModel = vm;

        // Act
        navService.NavigateTo<DashboardViewModel>();

        // Assert
        _ = navService.CurrentViewModel.Should().NotBeNull();
        _ = navService.CurrentViewModel.Should().BeOfType<DashboardViewModel>();
        _ = navigatedViewModel.Should().Be(navService.CurrentViewModel);
    }

    /// <summary>
    /// Verifies that successful login correctly populates user and tenant contexts with authenticated values.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task LoginViewModel_SuccessfulAuthentication_ShouldPopulateUserAndTenantContexts()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var expectedTenantId = Guid.NewGuid();
        var userContext = new DesktopUserContext();
        var tenantContext = new DesktopTenantContext();
        var localizer = new DesktopLocalizer();

        using var navService = new NavigationService(new ServiceCollection().BuildServiceProvider());

        var mediatorMock = new FakeMediator(new AuthResponseDto(
            "fake-access-token",
            "fake-refresh-token",
            DateTimeOffset.UtcNow.AddHours(1),
            expectedUserId,
            expectedTenantId,
            "it-CH"));

        var loginVm = new LoginViewModel(mediatorMock, userContext, tenantContext, localizer, navService)
        {
            TenantSlug = "acme-tenant",
            Email = "user@acme.com",
            Password = "SecurePassword123!",
        };

        var loginEventFired = false;
        loginVm.LoginSucceeded += () => loginEventFired = true;

        // Act
        await loginVm.SignInCommand.ExecuteAsync(parameter: null);

        // Assert
        _ = loginEventFired.Should().BeTrue();
        _ = tenantContext.TenantId.Should().Be(expectedTenantId);
        _ = userContext.UserId.Should().Be(expectedUserId);
        _ = userContext.TenantId.Should().Be(expectedTenantId);
        _ = userContext.Culture.Should().Be("it-CH");
    }

    private sealed class FakeMediator(AuthResponseDto response) : IMediator
    {
        private readonly AuthResponseDto _response = response;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => Task.FromResult((TResponse)(object)Result.Success(_response));

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(Result.Success(_response));

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
