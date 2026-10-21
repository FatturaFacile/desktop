// <copyright file="UserRole.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies user role.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Super Admin.
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Admin.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Accountant.
    /// </summary>
    Accountant = 3,

    /// <summary>
    /// Field Technician.
    /// </summary>
    FieldTechnician = 4,

    /// <summary>
    /// Sales Representative.
    /// </summary>
    SalesRepresentative = 5,

    /// <summary>
    /// Hr Manager.
    /// </summary>
    HrManager = 6,
}
