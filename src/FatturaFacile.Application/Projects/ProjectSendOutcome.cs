// <copyright file="ProjectSendOutcome.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

internal enum ProjectSendOutcome
{
    Sent = 0,

    Refused = 1,

    Retry = 2,
}
