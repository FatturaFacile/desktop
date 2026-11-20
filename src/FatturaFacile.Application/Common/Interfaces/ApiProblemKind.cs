// <copyright file="ApiProblemKind.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// The reasons the API refuses the current session or request, as the stable code of a 403 problem or a 401.
/// </summary>
public enum ApiProblemKind
{
    /// <summary>
    /// 401: the token was revoked or expired; the session is over.
    /// </summary>
    SessionRevoked = 0,

    /// <summary>
    /// 403 permission_denied: the cached permissions are stale.
    /// </summary>
    PermissionDenied = 1,

    /// <summary>
    /// 403 mfa_required: a second factor must be set up or presented.
    /// </summary>
    MfaRequired = 2,

    /// <summary>
    /// 403 company_access_revoked: the member lost access to the company.
    /// </summary>
    CompanyAccessRevoked = 3,
}
