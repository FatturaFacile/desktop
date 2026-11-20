// <copyright file="SyncViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using FatturaFacile.Application.Sync;
using FatturaFacile.Infrastructure.Sync;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the sync sign-in dialog view model.
/// </summary>
public sealed class SyncViewModelTests : IDisposable
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISyncApiClient> api = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private ServiceProvider? provider;

    /// <summary>
    /// Rejected credentials keep the user signed out and show the localized message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Connect_RejectedCredentials_ShowsLoginFailure()
    {
        _ = api.Setup(a => a.LoginAsync("a@b.ch", "x", Any)).ReturnsAsync(value: false);
        using var coordinator = CreateCoordinator(enabled: true);
        var vm = CreateViewModel(coordinator, enabled: true);
        vm.Email = "a@b.ch";
        vm.Password = "x";

        await vm.ConnectAsync();

        Assert.False(vm.IsConnected);
        Assert.Equal(loc["Sync_LoginFailed"], vm.StatusMessage);
        Assert.Empty(vm.Password);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A network failure during sign-in is reported instead of thrown.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Connect_NetworkDown_ReportsError()
    {
        _ = api.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("no route"));
        using var coordinator = CreateCoordinator(enabled: true);
        var vm = CreateViewModel(coordinator, enabled: true);

        await vm.ConnectAsync();

        Assert.False(vm.IsConnected);
        Assert.Contains("no route", vm.StatusMessage, StringComparison.Ordinal);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A pass without a valid session asks the user to sign in again.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncNow_WithoutSession_AsksToSignIn()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: false);
        using var coordinator = CreateCoordinator(enabled: true);
        var vm = CreateViewModel(coordinator, enabled: true);

        await vm.SyncNowAsync();

        Assert.Equal(loc["Sync_AuthRequired"], vm.StatusMessage);
        Assert.False(vm.IsConnected);
    }

    /// <summary>
    /// Disconnecting clears the session and the connected flag.
    /// </summary>
    [Fact]
    public void Disconnect_ClearsSession()
    {
        _ = api.SetupGet(a => a.IsAuthenticated).Returns(value: true);
        using var coordinator = CreateCoordinator(enabled: true);
        var vm = CreateViewModel(coordinator, enabled: true);
        Assert.True(vm.IsConnected);

        vm.Disconnect();

        api.Verify(a => a.Logout(), Times.Once);
        Assert.False(vm.IsConnected);
    }

    /// <summary>
    /// The dialog opens and closes and the password never outlives it.
    /// </summary>
    [Fact]
    public void Dialog_OpenAndClose_ClearsPassword()
    {
        using var coordinator = CreateCoordinator(enabled: true);
        var vm = CreateViewModel(coordinator, enabled: true);

        vm.OpenDialog();
        vm.Password = "secret";
        Assert.True(vm.IsDialogOpen);

        vm.CloseDialog();

        Assert.False(vm.IsDialogOpen);
        Assert.Empty(vm.Password);
    }

    /// <summary>
    /// Without a configured URL the sync controls are not available.
    /// </summary>
    [Fact]
    public void NoBaseUrl_IsNotAvailable()
    {
        using var coordinator = CreateCoordinator(enabled: false);

        Assert.False(CreateViewModel(coordinator, enabled: false).IsAvailable);
    }

    /// <inheritdoc/>
    public void Dispose() => provider?.Dispose();

    private static IOptions<SyncOptions> Options(bool enabled)
        => Microsoft.Extensions.Options.Options.Create(new SyncOptions { BaseUrl = enabled ? "https://api.test/" : string.Empty });

    private SyncCoordinator CreateCoordinator(bool enabled)
    {
        var services = new ServiceCollection();
        _ = services.AddSingleton(api.Object);
        _ = services.AddSingleton(new Mock<ISyncStore>().Object);
        _ = services.AddSingleton(TimeProvider.System);
        _ = services.AddScoped<SyncEngine>(sp => new SyncEngine(
            sp.GetRequiredService<ISyncApiClient>(),
            sp.GetRequiredService<ISyncStore>(),
            [],
            sp.GetRequiredService<TimeProvider>()));
        provider?.Dispose();
        provider = services.BuildServiceProvider();
        return new SyncCoordinator(provider.GetRequiredService<IServiceScopeFactory>(), Options(enabled));
    }

    private SyncViewModel CreateViewModel(SyncCoordinator coordinator, bool enabled)
        => new(api.Object, coordinator, Options(enabled), loc);
}
