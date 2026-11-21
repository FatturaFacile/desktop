// <copyright file="SecurityApiRoutes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Routes of the web-api security surface.
/// </summary>
public static class SecurityApiRoutes
{
    /// <summary>
    /// POST: uploads a batch of offline audit entries atomically.
    /// </summary>
    public const string OfflineAuditBatches = "/api/v1/audit/offline-batches";

    /// <summary>
    /// POST: reveals a masked personal-data field (requires a justification).
    /// </summary>
    public const string PiiUnmask = "/api/v1/security/pii/unmask";

    /// <summary>
    /// The origin marker sent with every batch uploaded from the desktop client.
    /// </summary>
    public const string DesktopOfflineOrigin = "desktop_offline";
}
