// <copyright file="RolePermissions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Security;

/// <summary>
/// Maps tenant roles to the permissions they grant (segregation of duties between sales, accounting and payroll).
/// </summary>
public static class RolePermissions
{
    private static readonly Permission[] _accountantExcluded =
        [Permission.PayrollRead, Permission.PayrollWrite, Permission.UsersManage, Permission.TimeRead, Permission.TimeManage];

    private static readonly Permission[] _payroll =
        [Permission.CommercialRead, Permission.AccountingRead, Permission.PayrollRead, Permission.PayrollWrite, Permission.TimeClock, Permission.TimeRead, Permission.TimeManage];

    private static readonly Permission[] _sales =
        [Permission.CommercialRead, Permission.MasterDataWrite, Permission.InvoicesWrite, Permission.InvoicesIssue, Permission.TimeClock];

    private static readonly Permission[] _viewer = [Permission.CommercialRead, Permission.AccountingRead, Permission.TimeClock];

    /// <summary>
    /// Determines whether a role grants a permission.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="permission">The permission.</param>
    /// <returns><see langword="true"/> when granted.</returns>
    public static bool Grants(UserRole role, Permission permission)
    {
        return role switch
        {
            UserRole.Administrator => true,
            UserRole.Accountant => !_accountantExcluded.Contains(permission),
            UserRole.Payroll => _payroll.Contains(permission),
            UserRole.Sales => _sales.Contains(permission),
            UserRole.Viewer => _viewer.Contains(permission),
            _ => false,
        };
    }
}
