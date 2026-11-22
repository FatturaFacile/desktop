// <copyright file="FieldServiceChecklistTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The default checklist of each work type, the same the server hands to the mobile clients in the offline bundle.
/// </summary>
internal static class FieldServiceChecklistTemplates
{
    private static readonly Dictionary<string, string[]> ByWorkType = new(StringComparer.Ordinal)
    {
        ["preventive"] = ["visual_inspection", "lubrication", "safety_devices", "functional_test"],
        ["corrective"] = ["fault_diagnosis", "repair_verified", "functional_test"],
        ["installation"] = ["installation_complete", "commissioning", "customer_briefing"],
        ["warranty"] = ["fault_diagnosis", "repair_verified", "functional_test"],
    };

    /// <summary>
    /// Gets the checklist items of a work type; an unknown type gets the corrective checklist.
    /// </summary>
    /// <param name="workType">The work type.</param>
    /// <returns>The checklist items.</returns>
    public static IReadOnlyList<string> For(string workType) => ByWorkType.GetValueOrDefault(workType, ByWorkType["corrective"]);
}
