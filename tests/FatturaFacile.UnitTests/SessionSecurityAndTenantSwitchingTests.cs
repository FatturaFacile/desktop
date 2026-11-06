// <copyright file="SessionSecurityAndTenantSwitchingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Tenants.Commands;
using FatturaFacile.Application.Features.Tenants.Notifications;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Services;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.Services;
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
        var userService = new CurrentUserService();
        userService.SignIn(new AuthenticatedUser(Guid.NewGuid(), "admin", "Admin", IsSystemAdministrator: false, new Dictionary<Guid, UserRole> { [targetId] = UserRole.Viewer }));
        var handler = new SwitchTenantCommandHandler(tenantRepo, tenantService, publisher, userService);

        var result = await handler.Handle(new SwitchTenantCommand(targetId), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(targetId, tenantService.TenantId);
        _ = Assert.Single(publisher.PublishedNotifications);
        _ = Assert.IsType<TenantChangedNotification>(publisher.PublishedNotifications[0]);
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
    /// Verifies that MainViewModel session locking and password unlocking update security states.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task MainViewModel_LockAndUnlockFlow_ModifiesStateCorrectly()
    {
        var tenantService = new CurrentTenantService();
        var userService = new CurrentUserService();
        userService.SignIn(new AuthenticatedUser(Guid.NewGuid(), "admin", "Admin", IsSystemAdministrator: true, new Dictionary<Guid, UserRole>()));
        var sender = new MockSender();
        var loc = LocalizationService.Instance;

        var modules = new ShellModules(
            new InvoicingViewModel(sender, new MockExportService(), tenantService, loc),
            new AccountingViewModel(sender, tenantService, loc),
            new ContactsViewModel(sender, tenantService, loc),
            new ProductsViewModel(sender, tenantService, loc),
            new PayrollViewModel(sender, tenantService, loc),
            new CompanySettingsViewModel(sender, loc),
            new UsersViewModel(sender, tenantService, loc));

        var mainVm = new MainViewModel(sender, userService, tenantService, loc, new MockFileDialogService(), new MockRestarter(), modules);

        Assert.True(mainVm.IsLoggedOut);

        mainVm.LockApplication();
        Assert.True(mainVm.IsLocked);

        mainVm.UnlockPassword = "correct password";
        await mainVm.UnlockApplicationAsync();
        Assert.False(mainVm.IsLocked);
        Assert.Empty(mainVm.UnlockPassword);

        mainVm.Logout();
        Assert.True(mainVm.IsLoggedOut);
        Assert.False(userService.IsAuthenticated);
    }

    private sealed class MockTenantRepository(Guid targetId) : IRepository<Tenant>
    {
        public IQueryable<Tenant> Query() => throw new NotImplementedException();

        public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == targetId)
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
        public List<object> PublishedNotifications { get; } = [];

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

            if (typeof(TResponse) == typeof(bool))
            {
                return Task.FromResult((TResponse)(object)true);
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

    private sealed class MockFileDialogService : IFileDialogService
    {
        public string? ShowSaveFile(string title, string filter, string defaultFileName) => null;

        public string? ShowOpenFile(string title, string filter) => null;
    }

    private sealed class MockRestarter : IApplicationRestarter
    {
        public void Restart()
        {
        }
    }
}
