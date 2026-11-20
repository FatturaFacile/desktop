// <copyright file="FieldPolicyEntry.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// A field policy of the signed-in user: how one field of one resource is shown.
/// </summary>
/// <param name="Resource">The resource, for example hr.employee.</param>
/// <param name="Field">The field name.</param>
/// <param name="Visibility">The visibility.</param>
/// <param name="Kind">The kind of value, amount or iban; it decides the mask.</param>
public sealed record FieldPolicyEntry(string Resource, string Field, FieldVisibility Visibility, string Kind);
