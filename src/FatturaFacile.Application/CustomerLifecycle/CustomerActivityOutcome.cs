// <copyright file="CustomerActivityOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// What happened to a quick action.
/// </summary>
public enum CustomerActivityOutcome
{
    /// <summary>
    /// The server accepted the activity.
    /// </summary>
    Sent = 0,

    /// <summary>
    /// The server was unreachable: the activity was saved locally and will be sent later.
    /// </summary>
    QueuedOffline = 1,
}
