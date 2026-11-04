// <copyright file="DatabaseProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Configuration;

/// <summary>
/// Supported relational database providers for the FatturaFacile ERP engine.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Embedded SQLite database engine, ideal for desktop, local, and testing environments.
    /// </summary>
    Sqlite = 0,

    /// <summary>
    /// PostgreSQL database engine, recommended for scalable multi-tenant client-server deployments.
    /// </summary>
    PostgreSQL = 1,

    /// <summary>
    /// Microsoft SQL Server database engine for enterprise corporate infrastructure.
    /// </summary>
    SqlServer = 2,
}
