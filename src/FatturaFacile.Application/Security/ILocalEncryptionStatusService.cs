// <copyright file="ILocalEncryptionStatusService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Reports whether the local SQLite storage is encrypted and its key is protected by Windows DPAPI.
/// </summary>
public interface ILocalEncryptionStatusService
{
    /// <summary>
    /// Inspects the local storage.
    /// </summary>
    /// <returns>The current status.</returns>
    public LocalEncryptionStatus GetStatus();
}
