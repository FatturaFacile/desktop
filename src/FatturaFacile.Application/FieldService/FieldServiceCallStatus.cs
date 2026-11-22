// <copyright file="FieldServiceCallStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// How an online call to the web-api ended.
/// </summary>
public enum FieldServiceCallStatus
{
    /// <summary>
    /// The server accepted the call.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// The server could not be reached; the local cache is used.
    /// </summary>
    Offline = 1,

    /// <summary>
    /// There is no session, or the server refused the credentials.
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The user may not use the Field Service module.
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The server validated the request and refused it (HTTP 4xx other than 401 and 403).
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// The server failed or answered something that could not be read.
    /// </summary>
    Failed = 5,
}
