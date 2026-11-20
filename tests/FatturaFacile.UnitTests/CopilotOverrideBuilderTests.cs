// <copyright file="CopilotOverrideBuilderTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using FatturaFacile.Application.Copilot;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the overrides sent when the user confirms a proposed action.
/// </summary>
public sealed class CopilotOverrideBuilderTests
{
    private static readonly CopilotProposedAction Action = new(
        "create_invoice",
        "invoicing",
        "pending",
        "Crea fattura",
        [
            new CopilotArgument("customer", "ACME", CopilotArgumentKind.Text),
            new CopilotArgument("items.0.quantity", "2", CopilotArgumentKind.Number),
            new CopilotArgument("items.0.billable", "true", CopilotArgumentKind.Boolean),
        ],
        new Dictionary<string, string>(),
        ["items.0.quantity", "items.0.billable"],
        null,
        null);

    /// <summary>
    /// Only changed fields that the server lists as editable are sent, converted to their JSON type.
    /// </summary>
    [Fact]
    public void Build_SendsOnlyChangedEditableFieldsWithTypes()
    {
        var overrides = CopilotOverrideBuilder.Build(Action, new Dictionary<string, string>
        {
            ["items.0.quantity"] = "3",
            ["items.0.billable"] = "false",
            ["customer"] = "Hacked AG",
        });

        Assert.Equal(2, overrides.Count);
        Assert.Equal(3m, overrides["items.0.quantity"]);
        Assert.False(Assert.IsType<bool>(overrides["items.0.billable"]));
        Assert.False(overrides.ContainsKey("customer"));
    }

    /// <summary>
    /// Untouched fields produce no overrides.
    /// </summary>
    [Fact]
    public void Build_UnchangedValues_ReturnsEmpty()
    {
        var overrides = CopilotOverrideBuilder.Build(Action, new Dictionary<string, string> { ["items.0.quantity"] = "2" });

        Assert.Empty(overrides);
    }
}
