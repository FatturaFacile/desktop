// <copyright file="SecurityViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the security view model (status, lock settings, queue and PII unmasking) and of its localized resources.
/// </summary>
public sealed class SecurityViewModelTests : IDisposable
{
    private readonly AuditTestDatabase database = new();
    private readonly FakeSecurityApiClient api = new();
    private readonly Mock<ISecuritySettingsStore> settings = new();
    private readonly Mock<ILocalEncryptionStatusService> encryption = new();
    private readonly ServiceProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityViewModelTests"/> class.
    /// </summary>
    public SecurityViewModelTests()
    {
        _ = settings.Setup(s => s.Load()).Returns(new SecuritySettings());
        _ = encryption.Setup(e => e.GetStatus()).Returns(new LocalEncryptionStatus(DatabaseFileEncrypted: false, FieldEncryptionActive: true, KeyProtectedByDpapi: true, "DPAPI-CurrentUser", "C:\\data\\db.sqlite"));

        ServiceCollection services = new();
        _ = services.AddSingleton<ILocalAuditQueue>(database.Queue);
        _ = services.AddScoped(_ => new AuditQueueSynchronizer(database.Queue, api, database.Clock));
        provider = services.BuildServiceProvider();
    }

    /// <summary>
    /// The justification must have at least ten characters after trimming.
    /// </summary>
    /// <param name="text">The typed text.</param>
    /// <param name="expected">Whether it is accepted.</param>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("123456789", false)]
    [InlineData("   123456789   ", false)]
    [InlineData("1234567890", true)]
    [InlineData("  customer asked  ", true)]
    public void PiiJustification_RequiresTenCharacters(string? text, bool expected)
    {
        Assert.Equal(expected, PiiUnmaskRules.IsJustificationValid(text));
        Assert.Equal(10, PiiUnmaskRules.MinimumJustificationLength);
    }

    /// <summary>
    /// The confirm command is disabled until the justification is long enough.
    /// </summary>
    [Fact]
    public void Confirm_IsDisabledUntilJustificationIsValid()
    {
        var vm = CreateViewModel();
        vm.OpenUnmaskDialog();

        vm.UnmaskJustification = "too short";
        Assert.False(vm.ConfirmUnmaskCommand.CanExecute(parameter: null));
        Assert.NotEmpty(vm.JustificationError);

        vm.UnmaskJustification = "customer asked for refund";
        Assert.True(vm.ConfirmUnmaskCommand.CanExecute(parameter: null));
        Assert.Empty(vm.JustificationError);
    }

    /// <summary>
    /// A confirmed unmask is logged in the signed local chain first, then sent with the trimmed justification, and the value is shown until hidden.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConfirmUnmask_AuditsThenRevealsAndCanBeHidden()
    {
        var vm = CreateViewModel();
        vm.OpenUnmaskDialog();
        vm.UnmaskEntityType = "contacts";
        vm.UnmaskEntityId = "c-1";
        vm.UnmaskField = "iban";
        vm.UnmaskJustification = "  customer asked for refund  ";

        await vm.ConfirmUnmaskCommand.ExecuteAsync(parameter: null);

        var request = Assert.Single(api.Unmasks);
        Assert.Equal(new PiiUnmaskRequest("contacts", "c-1", "iban", "customer asked for refund"), request);
        Assert.True(vm.HasRevealedValue);
        Assert.Equal("secret-value", vm.RevealedValue);
        Assert.Empty(vm.UnmaskJustification);

        var logged = Assert.Single(await database.Queue.GetPendingAsync(10, CancellationToken.None));
        Assert.Equal("pii_unmask_requested", logged.Action);
        Assert.Equal("contacts", logged.EntityType);
        Assert.Contains("customer asked for refund", logged.PayloadJson, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-value", logged.PayloadJson, StringComparison.Ordinal);

        vm.HideRevealedCommand.Execute(parameter: null);
        Assert.False(vm.HasRevealedValue);
        Assert.Empty(vm.RevealedValue);
    }

    /// <summary>
    /// When the server cannot be reached nothing is revealed and the user is told the request was only recorded locally.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConfirmUnmask_ServerUnreachable_RevealsNothing()
    {
        api.OnUnmask = static _ => new PiiUnmaskOutcome(SyncSendResult.Transient, Error: "offline");
        var vm = CreateViewModel();
        vm.UnmaskEntityType = "contacts";
        vm.UnmaskEntityId = "c-1";
        vm.UnmaskField = "iban";
        vm.UnmaskJustification = "customer asked for refund";

        await vm.ConfirmUnmaskCommand.ExecuteAsync(parameter: null);

        Assert.False(vm.HasRevealedValue);
        Assert.NotEmpty(vm.UnmaskStatusMessage);
        _ = Assert.Single(await database.Queue.GetPendingAsync(10, CancellationToken.None));
    }

    /// <summary>
    /// Missing entity data never reaches the server nor the audit chain.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ConfirmUnmask_MissingFields_DoesNothing()
    {
        var vm = CreateViewModel();
        vm.UnmaskJustification = "customer asked for refund";

        await vm.ConfirmUnmaskCommand.ExecuteAsync(parameter: null);

        Assert.Empty(api.Unmasks);
        Assert.Empty(await database.Queue.GetPendingAsync(10, CancellationToken.None));
        Assert.NotEmpty(vm.UnmaskStatusMessage);
    }

    /// <summary>
    /// Loading shows the encryption status, the saved settings and the queue state.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_PopulatesStatusSettingsAndQueue()
    {
        _ = settings.Setup(s => s.Load()).Returns(new SecuritySettings { LockTimeoutMinutes = 30, IdleLockEnabled = false });
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update"), CancellationToken.None);
        var vm = CreateViewModel();

        await vm.LoadCommand.ExecuteAsync(parameter: null);

        Assert.True(vm.IsProtected);
        Assert.Equal("DPAPI-CurrentUser", vm.ProtectionScheme);
        Assert.Equal(30, vm.LockTimeoutMinutes);
        Assert.False(vm.IdleLockEnabled);
        Assert.Equal(1, vm.PendingCount);
        Assert.True(vm.ChainIsIntact);
        var row = Assert.Single(vm.Entries);
        Assert.Equal(1, row.Sequence);
        Assert.Equal("contacts/c-1", row.Entity);
    }

    /// <summary>
    /// Saving clamps the timeout, persists it and records the change in the audit chain.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SaveSettings_ClampsPersistsAndAudits()
    {
        SecuritySettings? saved = null;
        _ = settings.Setup(s => s.Save(It.IsAny<SecuritySettings>())).Callback<SecuritySettings>(s => saved = s);
        var vm = CreateViewModel();
        vm.LockTimeoutMinutes = 100000;

        await vm.SaveSettingsCommand.ExecuteAsync(parameter: null);

        Assert.NotNull(saved);
        Assert.Equal(SecuritySettings.MaxLockTimeoutMinutes, saved.LockTimeoutMinutes);
        Assert.Equal(SecuritySettings.MaxLockTimeoutMinutes, vm.LockTimeoutMinutes);
        Assert.Equal("security_settings_changed", Assert.Single(await database.Queue.GetPendingAsync(10, CancellationToken.None)).Action);
    }

    /// <summary>
    /// Syncing the queue from the view uploads the pending entries and updates the counters.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SyncAuditNow_UploadsAndRefreshesCounters()
    {
        _ = await database.Queue.AppendAsync(new AuditEventInput("contacts", "c-1", "update"), CancellationToken.None);
        var vm = CreateViewModel();

        await vm.SyncAuditNowCommand.ExecuteAsync(parameter: null);

        _ = Assert.Single(api.Batches);
        Assert.Equal(0, vm.PendingCount);
        Assert.Equal(1, vm.SyncedCount);
    }

    /// <summary>
    /// Every security string exists in all supported languages (it is the neutral resource; de-CH and fr-CH use the German and French resources).
    /// </summary>
    /// <param name="key">The resource key.</param>
    [Theory]
    [InlineData("Nav_Security")]
    [InlineData("Security_Title")]
    [InlineData("Security_Encryption_Header")]
    [InlineData("Security_KeyDpapi")]
    [InlineData("Security_Lock_Timeout")]
    [InlineData("Security_Queue_ChainBroken")]
    [InlineData("Security_Pii_Confirm")]
    [InlineData("Security_Pii_JustificationShort")]
    public void SecurityStrings_ExistInAllLanguages(string key)
    {
        Assert.False(string.IsNullOrWhiteSpace(Strings.ResourceManager.GetString(key, new CultureInfo("it-CH"))), $"{key} is missing in the neutral (Italian) resources.");

        foreach (var name in new[] { "de-CH", "fr-CH", "en" })
        {
            var culture = new CultureInfo(name);
            var dictionary = LoadSatellite(culture) ?? LoadSatellite(culture.Parent);

            Assert.NotNull(dictionary);
            Assert.True(dictionary.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value), $"{key} is missing for {name}.");
        }

        Assert.NotEqual(LoadSatellite(new CultureInfo("de"))!["Security_Pii_Confirm"], LoadSatellite(new CultureInfo("fr"))!["Security_Pii_Confirm"], StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        provider.Dispose();
        database.Dispose();
    }

    private static Dictionary<string, string>? LoadSatellite(CultureInfo culture)
    {
        try
        {
            using var stream = typeof(Strings).Assembly.GetSatelliteAssembly(culture).GetManifestResourceStream("FatturaFacile.Wpf.Localization.Strings.resources");
            if (stream is null)
            {
                return null;
            }

            using ResourceReader reader = new(stream);
            return reader.Cast<DictionaryEntry>()
                .Where(static entry => entry.Key is string && entry.Value is string)
                .ToDictionary(static entry => (string)entry.Key, static entry => (string)entry.Value!, StringComparer.Ordinal);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private SecurityViewModel CreateViewModel()
        => new(encryption.Object, settings.Object, api, provider.GetRequiredService<IServiceScopeFactory>(), LocalizationService.Instance);
}
