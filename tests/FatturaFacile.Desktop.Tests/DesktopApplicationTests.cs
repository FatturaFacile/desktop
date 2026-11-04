using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FatturaFacile.Application.Shared.Context;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Desktop.Wpf.Localization;
using FatturaFacile.Desktop.Wpf.Services;
using FatturaFacile.Desktop.Wpf.ViewModels;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Application.DTOs;
using Xunit;

namespace FatturaFacile.Desktop.Tests;

public class DesktopApplicationTests
{
    [Fact]
    public void Localizer_ShouldTranslateAndSwitchCulturesDynamically()
    {
        // Arrange
        var localizer = new DesktopLocalizer();

        // Act & Assert - Default EN
        localizer.SetCulture("en");
        localizer["Nav.Invoices"].Should().Be("Invoices");
        localizer["Dashboard.TotalSales"].Should().Be("Total Revenue");

        // Act & Assert - Italian
        localizer.SetCulture("it");
        localizer["Nav.Invoices"].Should().Be("Fatture");
        localizer["Dashboard.TotalSales"].Should().Be("Fatturato Totale");

        // Act & Assert - German
        localizer.SetCulture("de");
        localizer["Nav.Invoices"].Should().Be("Rechnungen");
        localizer["Dashboard.TotalSales"].Should().Be("Gesamtumsatz");

        // Act & Assert - French
        localizer.SetCulture("fr");
        localizer["Nav.Invoices"].Should().Be("Factures");
        localizer["Dashboard.TotalSales"].Should().Be("Chiffre d'Affaires Total");
    }

    [Fact]
    public void NavigationService_ShouldTransitionViewModelsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var localizer = new DesktopLocalizer();
        var userContext = new DesktopUserContext();
        var tenantContext = new DesktopTenantContext();

        services.AddSingleton<IDesktopLocalizer>(localizer);
        services.AddSingleton(userContext);
        services.AddSingleton(tenantContext);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<DashboardViewModel>();

        var provider = services.BuildServiceProvider();
        var navService = provider.GetRequiredService<INavigationService>();

        ViewModelBase? navigatedViewModel = null;
        navService.CurrentViewModelChanged += vm => navigatedViewModel = vm;

        // Act
        navService.NavigateTo<DashboardViewModel>();

        // Assert
        navService.CurrentViewModel.Should().NotBeNull();
        navService.CurrentViewModel.Should().BeOfType<DashboardViewModel>();
        navigatedViewModel.Should().Be(navService.CurrentViewModel);
    }

    [Fact]
    public async Task LoginViewModel_SuccessfulAuthentication_ShouldPopulateUserAndTenantContexts()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var expectedTenantId = Guid.NewGuid();
        var userContext = new DesktopUserContext();
        var tenantContext = new DesktopTenantContext();
        var localizer = new DesktopLocalizer();

        var mediatorMock = new FakeMediator(new AuthResponseDto(
            AccessToken: "test-token",
            RefreshToken: "test-refresh",
            ExpiresAtUtc: DateTimeOffset.UtcNow.AddHours(1),
            UserId: expectedUserId,
            TenantId: expectedTenantId,
            PreferredLanguage: "de"
        ));

        var navServiceMock = new FakeNavigationService();

        var loginVm = new LoginViewModel(
            mediatorMock,
            userContext,
            tenantContext,
            localizer,
            navServiceMock
        )
        {
            TenantSlug = "test-corp",
            Email = "user@testcorp.ch",
            Password = "SecurePassword123!"
        };

        var loginSucceededTriggered = false;
        loginVm.LoginSucceeded += () => loginSucceededTriggered = true;

        // Act
        await loginVm.SignInCommand.ExecuteAsync(null);

        // Assert
        loginSucceededTriggered.Should().BeTrue();
        tenantContext.TenantId.Should().Be(expectedTenantId);
        userContext.UserId.Should().Be(expectedUserId);
        userContext.TenantId.Should().Be(expectedTenantId);
        userContext.Culture.Should().Be("de");
        localizer.CurrentCulture.Should().Be("de");
        navServiceMock.LastNavigatedType.Should().Be(typeof(DashboardViewModel));
    }

    [Fact]
    public void InvoiceDetailViewModel_SwissRoundingRule_ShouldRoundToNearestFiveCentimes()
    {
        // Arrange
        var userContext = new DesktopUserContext();
        var tenantContext = new DesktopTenantContext();
        var localizer = new DesktopLocalizer();
        var mediatorMock = new FakeMediator(null!);
        var navServiceMock = new FakeNavigationService();

        var detailVm = new InvoiceDetailViewModel(
            mediatorMock,
            tenantContext,
            localizer,
            navServiceMock
        )
        {
            Currency = "CHF"
        };

        // Act - Total: 1 item at 10.02 with 8.1% VAT -> Net 10.02, Tax 0.81, Unrounded 10.83 -> Swiss 5c rounded: 10.85
        detailVm.Items.Clear();
        detailVm.Items.Add(new InvoiceItemRowModel(detailVm)
        {
            Description = "Test item",
            Quantity = 1,
            UnitPrice = 10.02m,
            TaxRate = 8.1m,
            DiscountPercentage = 0m
        });
        detailVm.RecalculateTotals();

        // Assert
        detailVm.TotalNet.Should().Be(10.02m);
        detailVm.TotalTax.Should().Be(0.81m);
        detailVm.TotalGross.Should().Be(10.85m);
        detailVm.RoundingAmount.Should().Be(0.02m);
    }

    private sealed class FakeMediator : IMediator
    {
        private readonly AuthResponseDto _response;

        public FakeMediator(AuthResponseDto response)
        {
            _response = response;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (typeof(TResponse) == typeof(Result<AuthResponseDto>))
            {
                var result = Result.Success(_response);
                return Task.FromResult((TResponse)(object)result);
            }

            throw new NotImplementedException();
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeNavigationService : INavigationService
    {
        public ViewModelBase? CurrentViewModel { get; private set; }
        public Type? LastNavigatedType { get; private set; }
        public event Action<ViewModelBase>? CurrentViewModelChanged;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastNavigatedType = typeof(TViewModel);
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentViewModel = viewModel;
            LastNavigatedType = viewModel.GetType();
            CurrentViewModelChanged?.Invoke(viewModel);
        }
    }
}
