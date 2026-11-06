// <copyright file="RecordCopier.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Reflection;

namespace FatturaFacile.Application.Common.Records;

/// <summary>
/// Copies the scalar properties of plain entities, so that the user interface always edits detached copies.
/// </summary>
public static class RecordCopier
{
    private static readonly string[] _protectedProperties = ["Id", "TenantId", "CreatedAt"];

    private static readonly Type[] _scalarTypes = [typeof(string), typeof(decimal), typeof(DateTime), typeof(Guid)];

    /// <summary>
    /// Returns a detached copy of an entity (scalar properties only).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The entity.</param>
    /// <returns>The copy.</returns>
    public static T Clone<T>(T source)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        T copy = new();
        CopyProperties(source, copy, includeKeys: true);
        return copy;
    }

    /// <summary>
    /// Copies the editable values of an entity onto another (identifier, company and creation date are kept).
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The edited values.</param>
    /// <param name="target">The tracked entity.</param>
    public static void CopyValues<T>(T source, T target)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        CopyProperties(source, target, includeKeys: false);
    }

    private static void CopyProperties<T>(T source, T target, bool includeKeys)
        where T : class
    {
        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite || !IsScalar(property.PropertyType))
            {
                continue;
            }

            if (!includeKeys && _protectedProperties.Contains(property.Name, StringComparer.Ordinal))
            {
                continue;
            }

            property.SetValue(target, property.GetValue(source));
        }
    }

    private static bool IsScalar(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive || underlying.IsEnum || _scalarTypes.Contains(underlying);
    }
}
