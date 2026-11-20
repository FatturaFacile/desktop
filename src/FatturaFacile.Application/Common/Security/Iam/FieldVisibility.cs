// <copyright file="FieldVisibility.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// What a field policy does with a value for the signed-in user.
/// </summary>
public enum FieldVisibility
{
    /// <summary>
    /// The value is shown as it is.
    /// </summary>
    Visible = 0,

    /// <summary>
    /// The value is replaced by a mask and flagged as reserved.
    /// </summary>
    Masked = 1,

    /// <summary>
    /// The field is omitted completely.
    /// </summary>
    Hidden = 2,
}
