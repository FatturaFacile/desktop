// <copyright file="SyncSession.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// In-memory web-api session (bearer token and server tenant). It is never persisted.
/// </summary>
public sealed class SyncSession
{
    private readonly Lock gate = new();
    private string? token;
    private string? serverTenantId;

    /// <summary>
    /// Gets a value indicating whether a token is available.
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            lock (gate)
            {
                return !string.IsNullOrEmpty(token);
            }
        }
    }

    /// <summary>
    /// Gets the current bearer token.
    /// </summary>
    public string? Token
    {
        get
        {
            lock (gate)
            {
                return token;
            }
        }
    }

    /// <summary>
    /// Gets the server tenant identifier sent as X-Tenant-Id.
    /// </summary>
    public string? ServerTenantId
    {
        get
        {
            lock (gate)
            {
                return serverTenantId;
            }
        }
    }

    /// <summary>
    /// Stores a session.
    /// </summary>
    /// <param name="newToken">The bearer token.</param>
    /// <param name="tenantId">The server tenant identifier, or null to keep the current one.</param>
    public void Set(string newToken, string? tenantId)
    {
        lock (gate)
        {
            token = newToken;
            serverTenantId = tenantId ?? serverTenantId;
        }
    }

    /// <summary>
    /// Clears the session.
    /// </summary>
    public void Clear()
    {
        lock (gate)
        {
            token = null;
            serverTenantId = null;
        }
    }
}
