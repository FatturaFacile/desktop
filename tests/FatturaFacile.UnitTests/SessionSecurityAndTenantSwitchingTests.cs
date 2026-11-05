// <copyright file="SessionSecurityAndTenantSwitchingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants.Commands;
using FatturaFacile.Application.Features.Tenants.Notifications;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Services;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Unit tests for session security overlays, biometric unlock flows, and dynamic tenant switching.
/// </summary>
public sealed class SessionSecurityAndTenantSwitchingTests
{
    /// <summary>
    /// Verifies that CurrentTenantService raises CurrentTenantChanged when tenant is switched.
    /// </summary>
    [Fact]
    public void CurrentTenantService_SetCurrentTenant_FiresCurrentTenantChangedEvent()
    {
        var tenantService = new CurrentTenantService();
        var eventFired = false;
        tenantService.CurrentTenantChanged += (_, _) => eventFired = true;

        var newTenantId = Guid.NewGuid();
        tenantService.SetCurrentTenant(newTenantId, "Acme Swiss SA", "CH", "CHF");

        Assert.True(eventFired);
        Assert.Equal(newTenantId, tenantService.TenantId);
        Assert.Equal("CH", tenantService.CountryCode);
        Assert.Equal("CHF", tenantService.DefaultCurrency);
    }

    /// <summary>
    /// Verifies that SwitchTenantCommandHandler updates tenant context and publishes notification.
    /// </summary>
    /// <returns>A task representing the test execution.</returns>
    [Fact]
    public async Task SwitchTenantCommandHandler_PublishesNotificationAndUpdatesContext()
    {
        var targetId = Guid.NewGuid();
        var tenantRepo = new MockTenantRepository(targetId);
        var tenantService = new CurrentTenantService();
        var publisher = new MockPublisher();
        var handler = new SwitchTenantCommandHandler(tenantRepo, tenantService, publisher);

        var result = await handler.Handle(new SwitchTenantCommand(targetId), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(targetId, tenantService.TenantId);
        Assert.Single(publisher.PublishedNotifications);
        Assert.IsType<TenantChangedNotification>(publisher.PublishedNotifications[0]);
        var notification = (TenantChangedNotification)publisher.PublishedNotifications[0];
        Assert.Equal(targetId, notification.TenantId);
        Assert.Equal("CH", notification.CountryCode);
        Assert.Equal("CHF", notification.DefaultCurrency);
    }

    /// <summary>
    /// Verifies that InvoicingViewModel toggles IsSwiss and IsItalian when tenant changes country code.
    /// </summary>
    [Fact]
    public void InvoicingViewModel_CountryCodeToggle_UpdatesJurisdictionProperties()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetCurrentTenant(Guid.NewGuid(), "Swiss Company", "CH", "CHF");

        var vm = new InvoicingViewModel(
            new MockSender(),
            new MockExportService(),
            tenantService,
            LocalizationService.Instance);

        Assert.True(vm.IsSwiss);
        Assert.False(vm.IsItalian);

        // Switch to Italian company
        tenantService.SetCurrentTenant(Guid.NewGuid(), "Italian Company", "IT", "EUR");

        Assert.False(vm.IsSwiss);
        Assert.True(vm.IsItalian);
    }

    /// <summary>
    /// Verifies that MainViewModel session locking and biometric unlocking update security states.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MainViewModel_LockAndUnlockFlow_ModifiesStateCorrectly()
    {
        var authService = new MockAuthService(shouldSucceed: true);
        var tenantService = new CurrentTenantService();
        var sender = new MockSender();
        var loc = LocalizationService.Instance;

        var invoicing = new InvoicingViewModel(sender, new MockExportService(), tenantService, loc);
        var accounting = new AccountingViewModel(sender, tenantService, loc);
        var contacts = new ContactsViewModel(sender, tenantService, loc);
        var products = new ProductsViewModel(sender, tenantService, loc);
        var payroll = new PayrollViewModel(sender, tenantService, loc);

        var mainVm = new MainViewModel(
            sender,
            authService,
            tenantService,
            loc,
            invoicing,
            accounting,
            contacts,
            products,
            payroll);

        // Verify hardware authentication availability
        Assert.True(await authService.IsHardwareAuthenticationAvailableAsync());

        // Initially not locked
        Assert.False(mainVm.IsLocked);

        // Lock application
        mainVm.LockApplication();
        Assert.True(mainVm.IsLocked);

        // Unlock with successful authentication
        await mainVm.UnlockApplicationAsync();
        Assert.False(mainVm.IsLocked);

        // Logout
        mainVm.Logout();
        Assert.True(mainVm.IsLoggedOut);
        Assert.False(mainVm.IsLocked);
    }

    private sealed class MockTenantRepository : IRepository<Tenant>
    {
        private readonly Guid _targetId;

        public MockTenantRepository(Guid targetId)
        {
            _targetId = targetId;
        }

        public IQueryable<Tenant> Query() => throw new NotImplementedException();

        public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == _targetId)
            {
                var tenant = new Tenant
                {
                    Id = id,
                    Name = "Test Swiss Tenant",
                    LegalName = "Test Swiss Tenant SA",
                    Country = "CH",
                    DefaultCurrency = "CHF",
                };
                return Task.FromResult<Tenant?>(tenant);
            }

            return Task.FromResult<Tenant?>(null);
        }

        public Task<IReadOnlyList<Tenant>> FindAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task AddAsync(Tenant entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public void Update(Tenant entity) => throw new NotImplementedException();

        public void Delete(Tenant entity) => throw new NotImplementedException();
    }

    private sealed class MockPublisher : IPublisher
    {
        public List<object> PublishedNotifications { get; } = new();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            PublishedNotifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            PublishedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class MockSender : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (typeof(TResponse) == typeof(IReadOnlyList<InvoiceDto>))
            {
                return Task.FromResult((TResponse)(object)new List<InvoiceDto>());
            }

            var result = typeof(TResponse) == typeof(IReadOnlyList<TenantDto>)
                ? (TResponse)(object)new List<TenantDto>()
                : default!;

            return Task.FromResult(result);
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class MockExportService : IDocumentExportService
    {
        public Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<byte>());

        public Task<byte[]> ExportInvoicesToExcelAsync(IEnumerable<InvoiceDto> invoices, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<byte>());
    }

    private sealed class MockAuthService : IUserAuthenticationService
    {
        private readonly bool _shouldSucceed;

        public MockAuthService(bool shouldSucceed)
        {
            _shouldSucceed = shouldSucceed;
        }

        public Task<bool> IsHardwareAuthenticationAvailableAsync() => Task.FromResult(_shouldSucceed);

        public Task<bool> AuthenticateCurrentUserAsync(string promptMessage)
        {
            ArgumentNullException.ThrowIfNull(promptMessage);
            return Task.FromResult(_shouldSucceed);
        }
    }
}
