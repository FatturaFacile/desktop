// <copyright file="CustomerActivityTypes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// The CRM activity types the quick actions create.
/// </summary>
public static class CustomerActivityTypes
{
    /// <summary>
    /// A logged visit.
    /// </summary>
    public const string Meeting = "meeting";

    /// <summary>
    /// A follow-up task.
    /// </summary>
    public const string FollowUp = "follow_up";

    /// <summary>
    /// The maximum length of the subject.
    /// </summary>
    public const int MaxSubjectLength = 200;
}
