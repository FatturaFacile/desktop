// <copyright file="RecordPermissions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Records;

/// <summary>
/// The read and write permissions of the master data records managed with the generic record requests.
/// </summary>
public static class RecordPermissions
{
    private static readonly Dictionary<Type, (Permission Read, Permission Write)> _permissions = new()
    {
        [typeof(Contact)] = (Permission.CommercialRead, Permission.MasterDataWrite),
        [typeof(Product)] = (Permission.CommercialRead, Permission.MasterDataWrite),
        [typeof(ChartOfAccount)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(BankAccount)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(BankTransaction)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(PurchaseInvoice)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(PurchaseInvoiceLine)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(VatReturn)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(FixedAsset)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(FixedAssetDepreciation)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(GeneralLedgerEntry)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(GeneralLedgerLine)] = (Permission.AccountingRead, Permission.AccountingWrite),
        [typeof(ProcessingActivity)] = (Permission.CompanySettingsManage, Permission.CompanySettingsManage),
    };

    /// <summary>
    /// Returns the permission needed to read records of a type.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <returns>The permission.</returns>
    /// <exception cref="NotSupportedException">When the type is not managed by the generic requests.</exception>
    public static Permission Read(Type type) => _permissions.TryGetValue(type, out var permissions)
        ? permissions.Read
        : throw new NotSupportedException($"Tipo di record non gestito: {type?.Name}.");

    /// <summary>
    /// Returns the permission needed to change records of a type.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <returns>The permission.</returns>
    /// <exception cref="NotSupportedException">When the type is not managed by the generic requests.</exception>
    public static Permission Write(Type type) => _permissions.TryGetValue(type, out var permissions)
        ? permissions.Write
        : throw new NotSupportedException($"Tipo di record non gestito: {type?.Name}.");
}
