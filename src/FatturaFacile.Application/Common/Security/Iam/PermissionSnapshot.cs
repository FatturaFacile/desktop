// <copyright file="PermissionSnapshot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Security.Iam;

/// <summary>
/// The effective permissions and field policies of one user in one tenant and company, as answered by
/// GET /api/v1/profile/permissions and kept for offline enforcement.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user identifier.</param>
/// <param name="CompanyId">The company the snapshot is limited to, or null for the whole tenant.</param>
/// <param name="Permissions">The granted permission keys; * and module.* are wildcards.</param>
/// <param name="FieldPolicies">The field policies.</param>
/// <param name="FetchedAt">When the snapshot was read from the server.</param>
public sealed record PermissionSnapshot(
    Guid TenantId,
    Guid UserId,
    Guid? CompanyId,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<FieldPolicyEntry> FieldPolicies,
    DateTimeOffset FetchedAt);
