// <copyright file="CopilotRouteResolver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Copilot;

/// <summary>
/// Maps the entity types and web routes carried by Copilot citations to the native desktop modules.
/// </summary>
public static class CopilotRouteResolver
{
    private static readonly Dictionary<string, string> Modules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["invoice"] = "Invoicing",
        ["invoices"] = "Invoicing",
        ["sales_invoice"] = "Invoicing",
        ["quote"] = "Invoicing",
        ["credit_note"] = "Invoicing",
        ["customer"] = "Contacts",
        ["supplier"] = "Contacts",
        ["contact"] = "Contacts",
        ["contacts"] = "Contacts",
        ["customers"] = "Contacts",
        ["suppliers"] = "Contacts",
        ["product"] = "Products",
        ["products"] = "Products",
        ["article"] = "Products",
        ["articles"] = "Products",
        ["account"] = "Accounting",
        ["accounts"] = "Accounting",
        ["journal_entry"] = "Accounting",
        ["accounting"] = "Accounting",
        ["general_ledger"] = "Accounting",
        ["employee"] = "Payroll",
        ["employees"] = "Payroll",
        ["payroll"] = "Payroll",
        ["payslip"] = "Payroll",
        ["time_tracking"] = "TimeTracking",
        ["time-tracking"] = "TimeTracking",
        ["purchase_invoice"] = "Finance",
        ["purchases"] = "Finance",
        ["bank_account"] = "Finance",
        ["bank"] = "Finance",
        ["vat_return"] = "Finance",
        ["vat"] = "Finance",
        ["fixed_asset"] = "Finance",
        ["finance"] = "Finance",
    };

    /// <summary>
    /// Resolves the native module that shows an entity.
    /// </summary>
    /// <param name="entityType">The entity type of the citation.</param>
    /// <param name="route">The route of the citation, e.g. "/invoices/42".</param>
    /// <returns>The module name understood by the shell navigation, or null when no native view exists.</returns>
    public static string? ResolveModule(string? entityType, string? route)
    {
        if (!string.IsNullOrWhiteSpace(entityType) && Modules.TryGetValue(entityType.Trim(), out var byType))
        {
            return byType;
        }

        foreach (var segment in (route ?? string.Empty).Split(['/', '?', '#'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (Modules.TryGetValue(segment, out var byRoute))
            {
                return byRoute;
            }
        }

        return null;
    }

    /// <summary>
    /// Derives the route (module key) the context sent with a prompt should carry for a shell module.
    /// </summary>
    /// <param name="moduleName">The shell module name.</param>
    /// <returns>The lower case route.</returns>
    public static string RouteOf(string moduleName)
    {
        ArgumentNullException.ThrowIfNull(moduleName);
        return "/" + moduleName.ToLowerInvariant();
    }
}
