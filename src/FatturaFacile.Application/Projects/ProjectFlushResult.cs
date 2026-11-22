// <copyright file="ProjectFlushResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// The outcome of replaying the project outbox.
/// </summary>
/// <param name="Sent">The entries delivered.</param>
/// <param name="Conflicts">The entries refused.</param>
/// <param name="Remaining">The entries still queued.</param>
/// <param name="Offline">Whether the network (or the sign-in) prevented a delivery.</param>
public sealed record ProjectFlushResult(int Sent, int Conflicts, int Remaining, bool Offline);
