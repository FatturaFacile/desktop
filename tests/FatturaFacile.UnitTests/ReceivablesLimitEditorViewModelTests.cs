// <copyright file="ReceivablesLimitEditorViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Receivables;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the credit limit dialog: defaults, prefill from an existing limit, local number validation and the submit.
/// </summary>
public sealed class ReceivablesLimitEditorViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private int completed;
    private ReceivablesActionResult result = ReceivablesFixtures.Sent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesLimitEditorViewModelTests"/> class.
    /// </summary>
    public ReceivablesLimitEditorViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SaveCreditLimitCommand>(), Any)).ReturnsAsync(() => result);
    }

    /// <summary>
    /// A new limit starts active, from today, in the default currency, on the first reaction policy.
    /// </summary>
    [Fact]
    public void Constructor_ForANewLimit_StartsWithTheDefaults()
    {
        var vm = Create();

        Assert.Equal("EUR", vm.Currency);
        Assert.True(vm.IsActive);
        Assert.False(vm.IsEdit);
        Assert.Equal(time.GetLocalNow().Date, vm.ValidFrom);
        Assert.Equal("warning", vm.SelectedPolicy?.Key);
        Assert.Equal(4, vm.PolicyChoices.Count);
    }

    /// <summary>
    /// Editing a limit prefills every field from it.
    /// </summary>
    [Fact]
    public void Constructor_ForAnExistingLimit_PrefillsTheFields()
    {
        var existing = ReceivablesFixtures.Limit(granted: 1500m, policy: CreditReactionPolicy.CreditHold) with { Currency = "CHF", InsurerName = "Coface", InsuredLimit = 800m, CoveragePercentage = 80m, ValidTo = ReceivablesFixtures.Today.AddDays(200), IsActive = false };

        var vm = Create(existing);

        Assert.True(vm.IsEdit);
        Assert.Equal("CHF", vm.Currency);
        Assert.True(ReceivablesFormat.TryParseAmount(vm.GrantedText, loc.CurrentCulture, out var granted));
        Assert.Equal(1500m, granted);
        Assert.Equal("credit_hold", vm.SelectedPolicy?.Key);
        Assert.Equal("Coface", vm.InsurerName);
        Assert.Equal("30", vm.MaxOverdueDaysText);
        Assert.False(vm.IsActive);
        _ = Assert.NotNull(vm.ValidTo);
    }

    /// <summary>
    /// A text that is not a number is refused before anything is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_InvalidNumbers_AreRefusedLocally()
    {
        var vm = Create();
        vm.GrantedText = "abc";

        await vm.SubmitCommand.ExecuteAsync(null);
        vm.GrantedText = "1000";
        vm.CoverageText = "x";
        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReceivablesService.LimitKey], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Bad, vm.ResultState);
        sender.Verify(s => s.Send(It.IsAny<SaveCreditLimitCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// Submitting sends the amounts, the insurance, the tolerance, the policy and the validity.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_SendsEveryField()
    {
        var vm = Create();
        vm.GrantedText = "2000";
        vm.Currency = "chf";
        vm.InsuredText = "1000";
        vm.CoverageText = "80";
        vm.InsurerName = "Coface";
        vm.MaxOverdueDaysText = "45";
        vm.ToleranceText = "100";
        vm.SelectedPolicy = vm.PolicyChoices.First(choice => string.Equals(choice.Key, "credit_hold", StringComparison.Ordinal));
        vm.ValidTo = vm.ValidFrom.AddDays(365);
        vm.Notes = "Reviewed";

        await vm.SubmitCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<SaveCreditLimitCommand>(c => IsFullLimit(c)), Any), Times.Once);
        Assert.Equal(loc["Receivables_Sent"], vm.ResultText);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// Editing sends the id of the limit; offline the outcome is the queued notice and the views still refresh.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Existing_SendsTheLimitIdAndQueuedShowsTheNotice()
    {
        result = ReceivablesFixtures.Queued();
        var existing = ReceivablesFixtures.Limit();
        var vm = Create(existing);

        await vm.SubmitCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<SaveCreditLimitCommand>(c => c.Request.LimitId == existing.Id), Any), Times.Once);
        Assert.Equal(loc["Receivables_Queued"], vm.ResultText);
        Assert.Equal(ReceivablesFormat.Warn, vm.ResultState);
        Assert.Equal(1, completed);
    }

    /// <summary>
    /// A refusal shows the reason and does not refresh the views.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Submit_Refused_DoesNotRefresh()
    {
        result = new ReceivablesActionResult(ReceivablesActionStatus.Rejected, ReceivablesService.InsurerKey);
        var vm = Create();
        vm.GrantedText = "1000";

        await vm.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReceivablesService.InsurerKey], vm.ResultText);
        Assert.Equal(0, completed);
    }

    private static bool IsFullLimit(SaveCreditLimitCommand command)
    {
        var request = command.Request;
        var amounts = request.GrantedLimit == 2000m && request.InsuredLimit == 1000m && request.CoveragePercentage == 80m;
        var terms = request.MaxOverdueDays == 45 && request.OverdueToleranceAmount == 100m && request.ReactionPolicy == CreditReactionPolicy.CreditHold;
        var text = string.Equals(request.Currency, "chf", StringComparison.Ordinal) && string.Equals(request.InsurerName, "Coface", StringComparison.Ordinal) && string.Equals(request.Notes, "Reviewed", StringComparison.Ordinal);
        var open = request.ValidTo is not null && request.LimitId is null;

        return amounts && terms && text && open;
    }

    private ReceivablesLimitEditorViewModel Create(CustomerCreditLimit? existing = null) => new(sender.Object, loc, time, CustomerId, "Rossi SA", existing, "EUR", () =>
    {
        completed++;

        return Task.CompletedTask;
    });
}
