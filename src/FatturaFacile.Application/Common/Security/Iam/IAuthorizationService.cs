// <copyright file="IAuthorizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// Deny-by-default authorization on permission keys (module.resource.action), backed by the cached permission
/// snapshot of the signed-in user, so it also works offline.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Gets the snapshot in force, or null when none is loaded.
    /// </summary>
    public PermissionSnapshot? Current { get; }

    /// <summary>
    /// Replaces the snapshot in force.
    /// </summary>
    /// <param name="snapshot">The snapshot, or null to deny everything (sign-out, revocation).</param>
    public void Apply(PermissionSnapshot? snapshot);

    /// <summary>
    /// Determines whether the permission is granted. Without a snapshot nothing is.
    /// </summary>
    /// <param name="permission">The permission key.</param>
    /// <returns><see langword="true"/> when granted.</returns>
    public bool IsGranted(string permission);

    /// <summary>
    /// Gets the most restrictive visibility the policies give a field.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="field">The field.</param>
    /// <returns>The visibility; <see cref="FieldVisibility.Visible"/> when no policy mentions the field.</returns>
    public FieldVisibility VisibilityOf(string resource, string field);
}
