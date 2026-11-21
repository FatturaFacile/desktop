// <copyright file="MesDomainTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the pure rules of the MES operator board: the state machine, the man / machine time algorithm, the keypad and the
/// counter format.
/// </summary>
public sealed class MesDomainTests
{
    private static readonly DateTime Start = new(2026, 6, 30, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Provides the allowed actions of every state.
    /// </summary>
    /// <returns>The test cases.</returns>
    public static TheoryData<MesPhaseStatus, MesAction[]> AllowedByStatus() => new()
    {
        { MesPhaseStatus.Available, [MesAction.TakeOver] },
        { MesPhaseStatus.Assigned, [MesAction.TakeOver, MesAction.StartSetup, MesAction.StartProduction] },
        { MesPhaseStatus.Setup, [MesAction.StartProduction, MesAction.Suspend, MesAction.ConsumeMaterial] },
        { MesPhaseStatus.Running, [MesAction.StartProduction, MesAction.Suspend, MesAction.DeclareProduction, MesAction.ConsumeMaterial, MesAction.Close] },
        { MesPhaseStatus.Suspended, [MesAction.TakeOver, MesAction.Resume, MesAction.DeclareProduction, MesAction.ConsumeMaterial, MesAction.Close] },
    };

    /// <summary>
    /// Each state offers exactly the actions the web-api state machine allows.
    /// </summary>
    /// <param name="status">The state.</param>
    /// <param name="expected">The allowed actions.</param>
    [Theory]
    [MemberData(nameof(AllowedByStatus))]
    public void AllowedActions_PerStatus_MatchTheStateMachine(MesPhaseStatus status, MesAction[] expected) =>
        Assert.Equal(expected, MesStateMachine.AllowedActions(status));

    /// <summary>
    /// A closed phase accepts nothing.
    /// </summary>
    [Fact]
    public void AllowedActions_ClosedPhase_IsEmpty()
    {
        Assert.Empty(MesStateMachine.AllowedActions(MesPhaseStatus.Closed));
        Assert.False(MesStateMachine.IsAllowed(MesAction.Close, MesPhaseStatus.Closed));
    }

    /// <summary>
    /// Resuming returns to the state the suspension interrupted, running when it is unknown.
    /// </summary>
    [Fact]
    public void Next_Resume_ReturnsToTheInterruptedState()
    {
        Assert.Equal(MesPhaseStatus.Setup, MesStateMachine.Next(MesAction.Resume, MesPhaseStatus.Suspended, MesPhaseStatus.Setup));
        Assert.Equal(MesPhaseStatus.Running, MesStateMachine.Next(MesAction.Resume, MesPhaseStatus.Suspended, resumeTo: null));
    }

    /// <summary>
    /// Taking over an available phase assigns it; declarations and lot consumptions do not change the state.
    /// </summary>
    [Fact]
    public void Next_TakeOverDeclareAndConsume_KeepOrAssignTheState()
    {
        Assert.Equal(MesPhaseStatus.Assigned, MesStateMachine.Next(MesAction.TakeOver, MesPhaseStatus.Available, resumeTo: null));
        Assert.Equal(MesPhaseStatus.Suspended, MesStateMachine.Next(MesAction.TakeOver, MesPhaseStatus.Suspended, resumeTo: null));
        Assert.Equal(MesPhaseStatus.Running, MesStateMachine.Next(MesAction.DeclareProduction, MesPhaseStatus.Running, resumeTo: null));
        Assert.Equal(MesPhaseStatus.Setup, MesStateMachine.Next(MesAction.ConsumeMaterial, MesPhaseStatus.Setup, resumeTo: null));
        Assert.Equal(MesPhaseStatus.Closed, MesStateMachine.Next(MesAction.Close, MesPhaseStatus.Running, resumeTo: null));
    }

    /// <summary>
    /// Man time is setup plus attended run, machine time is setup plus every run, and the suspension is downtime for both.
    /// </summary>
    [Fact]
    public void Summarize_MixedIntervals_SeparatesManMachineAndDowntime()
    {
        MesInterval[] intervals =
        [
            new(MesTimeCategory.Setup, "op-1", MachineAttended: true, Start, Start.AddMinutes(10)),
            new(MesTimeCategory.Run, "op-1", MachineAttended: true, Start.AddMinutes(10), Start.AddMinutes(40)),
            new(MesTimeCategory.Suspension, "op-1", MachineAttended: false, Start.AddMinutes(40), Start.AddMinutes(45)),
            new(MesTimeCategory.Run, "op-1", MachineAttended: false, Start.AddMinutes(45), Start.AddMinutes(65)),
        ];

        var summary = MesTimeCalculator.Summarize(intervals, Start.AddHours(3));

        Assert.Equal(10 * 60, summary.SetupSeconds);
        Assert.Equal(30 * 60, summary.AttendedRunSeconds);
        Assert.Equal(20 * 60, summary.UnattendedRunSeconds);
        Assert.Equal(5 * 60, summary.DowntimeSeconds);
        Assert.Equal(40 * 60, summary.ManSeconds);
        Assert.Equal(60 * 60, summary.MachineSeconds);
    }

    /// <summary>
    /// An open interval runs up to the requested instant; an interval that ends before it starts counts nothing.
    /// </summary>
    [Fact]
    public void Summarize_OpenAndInvertedIntervals_AreBounded()
    {
        MesInterval[] intervals =
        [
            new(MesTimeCategory.Run, OperatorId: null, MachineAttended: true, Start, EndedAtUtc: null),
            new(MesTimeCategory.Setup, OperatorId: null, MachineAttended: true, Start.AddMinutes(30), Start.AddMinutes(20)),
        ];

        var summary = MesTimeCalculator.Summarize(intervals, Start.AddSeconds(90));

        Assert.Equal(90, summary.AttendedRunSeconds);
        Assert.Equal(0, summary.SetupSeconds);
    }

    /// <summary>
    /// Advancing an open interval adds whole seconds to the counter of its category and ignores negative time.
    /// </summary>
    [Fact]
    public void Advance_OpenInterval_AddsToTheRightCounter()
    {
        var run = new MesOpenInterval(MesTimeCategory.Run, Start, MachineAttended: false, Reason: null);
        var suspension = new MesOpenInterval(MesTimeCategory.Suspension, Start, MachineAttended: false, MesSuspensionReason.Break);

        Assert.Equal(7, MesTimeSummary.Empty.Advance(run, 7.9).UnattendedRunSeconds);
        Assert.Equal(3, MesTimeSummary.Empty.Advance(suspension, 3).DowntimeSeconds);
        Assert.Equal(MesTimeSummary.Empty, MesTimeSummary.Empty.Advance(run, -5));
        Assert.Equal(MesTimeSummary.Empty, MesTimeSummary.Empty.Advance(open: null, 30));
    }

    /// <summary>
    /// The keypad appends digits, takes one decimal point and caps the decimals.
    /// </summary>
    [Fact]
    public void Keypad_TypingDigits_BuildsTheQuantity()
    {
        var typed = "0";
        foreach (var key in new[] { "1", "2", ".", "5", ".", "7", "5", "9", "1" })
        {
            typed = MesKeypad.Apply(typed, key);
        }

        Assert.Equal("12.5759", typed);
        Assert.Equal(12.5759m, MesKeypad.Parse(typed));
    }

    /// <summary>
    /// Back removes the last character down to zero, clear resets, unknown keys are ignored and the integer part is capped.
    /// </summary>
    [Fact]
    public void Keypad_BackClearAndLimits_Behave()
    {
        Assert.Equal("1", MesKeypad.Apply("12", MesKeypad.Back));
        Assert.Equal("0", MesKeypad.Apply("7", MesKeypad.Back));
        Assert.Equal("0", MesKeypad.Apply("987.5", MesKeypad.Clear));
        Assert.Equal("12", MesKeypad.Apply("12", "x"));
        Assert.Equal("123456789", MesKeypad.Apply("123456789", "0"));
        Assert.Equal("5", MesKeypad.Apply("0", "5"));
        Assert.Equal("0.", MesKeypad.Apply("0", "."));
    }

    /// <summary>
    /// The quick increments add whole pieces and never go below zero.
    /// </summary>
    [Fact]
    public void Keypad_Add_IncrementsAndFloorsAtZero()
    {
        Assert.Equal("15", MesKeypad.Add("5", 10));
        Assert.Equal("0", MesKeypad.Add("5", -10));
        Assert.Equal("1", MesKeypad.Add("not a number", 1));
    }

    /// <summary>
    /// Counters show hours, minutes and seconds, with hours beyond one day and no negative values.
    /// </summary>
    /// <param name="seconds">The duration.</param>
    /// <param name="expected">The text.</param>
    [Theory]
    [InlineData(0, "00:00:00")]
    [InlineData(3909, "01:05:09")]
    [InlineData(360000, "100:00:00")]
    [InlineData(-5, "00:00:00")]
    public void DurationFormat_Seconds_AreFormatted(long seconds, string expected) =>
        Assert.Equal(expected, MesDurationFormat.Format(seconds));

    /// <summary>
    /// The wire names of the enums are snake_case and round-trip.
    /// </summary>
    [Fact]
    public void Wire_Enums_RoundTripThroughSnakeCase()
    {
        Assert.Equal("machine_breakdown", MesWire.ToWire(MesSuspensionReason.MachineBreakdown));
        Assert.Equal("declare_production", MesWire.ToWire(MesAction.DeclareProduction));
        Assert.True(MesWire.TryParse("tool_change", out MesSuspensionReason reason));
        Assert.Equal(MesSuspensionReason.ToolChange, reason);
        Assert.False(MesWire.TryParse("nonsense", out MesSuspensionReason _));
    }

    /// <summary>
    /// The take-over command carries the operator and the declaration carries the quantities as dotted decimal strings.
    /// </summary>
    [Fact]
    public void Commands_Payloads_UseTheWebApiFieldNames()
    {
        var operationId = Guid.NewGuid();
        var take = MesCommand.TakeOver(operationId, phaseId: null, Guid.Empty, Start);
        var declare = MesCommand.Declare(MesTestRows.Row(MesPhaseStatus.Running), 12.5m, 2m, MesDefectReason.Surface, Start);

        Assert.Equal(Guid.Empty.ToString("D"), take.Text(MesCommand.OperatorField));
        Assert.Equal("12.5", declare.Text(MesCommand.GoodField));
        Assert.Equal("2", declare.Text(MesCommand.ScrapField));
        Assert.Equal("surface", declare.Text(MesCommand.DefectField));
        Assert.Equal(12.5m, declare.Amount(MesCommand.GoodField));
        Assert.NotEqual(take.Id, declare.Id);
    }
}
