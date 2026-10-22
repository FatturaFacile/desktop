// <copyright file="AiLayoutControlTools.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.ComponentModel;

using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Wpf;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.LayoutControl;

namespace FatturaFacile.Tools;

/// <summary>
/// AI tools providing automated field population for DevExpress LayoutControls.
/// </summary>
public static class AiLayoutControlTools
{
    [AIIntegrationTool("LayoutControl_Populate")]
    [Description("Populate a LayoutControl's fields from a JSON object. Returns a text with the operation result.")]
    public static async Task<string> Populate(
        [AIIntegrationToolTarget("Target LayoutControl to fill.")]
        LayoutControl layoutControl,
        [Description("Field values as a JSON object, for example: { \"FirstName\": \"John\", \"LastName\": \"Doe\" }")]
        string data)
    {
        ArgumentNullException.ThrowIfNull(layoutControl);

        var behaviors = Interaction.GetBehaviors(layoutControl);

        var smartPasteBehavior = behaviors.OfType<SmartPasteBehavior>().FirstOrDefault();

        try
        {
            await smartPasteBehavior.SmartPasteAsync(data);
            return "Fields populated in the LayoutControl.";
        }
        catch
        {
            return "Error: failed to populate fields in the LayoutControl.";
        }
    }
}
