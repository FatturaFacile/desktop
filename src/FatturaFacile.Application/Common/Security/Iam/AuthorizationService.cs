// <copyright file="AuthorizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// Holds the permission snapshot in force and answers authorization questions from it (deny by default).
/// </summary>
public sealed class AuthorizationService : IAuthorizationService
{
    private volatile PermissionSnapshot? _current;

    /// <inheritdoc/>
    public PermissionSnapshot? Current => _current;

    /// <inheritdoc/>
    public void Apply(PermissionSnapshot? snapshot) => _current = snapshot;

    /// <inheritdoc/>
    public bool IsGranted(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        var snapshot = _current;
        if (snapshot is null)
        {
            return false;
        }

        if (snapshot.Permissions.Contains("*", StringComparer.Ordinal) || snapshot.Permissions.Contains(permission, StringComparer.Ordinal))
        {
            return true;
        }

        var dot = permission.IndexOf('.', StringComparison.Ordinal);
        return dot > 0 && snapshot.Permissions.Contains($"{permission.AsSpan(0, dot)}.*", StringComparer.Ordinal);
    }

    /// <inheritdoc/>
    public FieldVisibility VisibilityOf(string resource, string field)
    {
        var result = FieldVisibility.Visible;

        foreach (var policy in _current?.FieldPolicies ?? [])
        {
            if (string.Equals(policy.Resource, resource, StringComparison.Ordinal)
                && string.Equals(policy.Field, field, StringComparison.Ordinal)
                && policy.Visibility > result)
            {
                result = policy.Visibility;
            }
        }

        return result;
    }
}
