// <copyright file="TemplateInstantiationRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Projects;

/// <summary>
/// What the user chooses when a project is created from a template (#186).
/// </summary>
/// <param name="TemplateId">The template project identifier.</param>
/// <param name="ProjectCode">The code of the new project.</param>
/// <param name="Name">The name of the new project.</param>
/// <param name="StartDate">The start of the new project; the template dates shift by the difference.</param>
/// <param name="CustomerId">The customer, if any.</param>
/// <param name="ProjectManagerId">The project manager, if any.</param>
/// <param name="LegalEntityId">The legal entity, if any.</param>
/// <param name="CupCode">The CUP code, if any.</param>
/// <param name="CigCode">The CIG code, if any.</param>
public sealed record TemplateInstantiationRequest(
    Guid TemplateId,
    string ProjectCode,
    string Name,
    DateOnly StartDate,
    Guid? CustomerId = null,
    Guid? ProjectManagerId = null,
    Guid? LegalEntityId = null,
    string? CupCode = null,
    string? CigCode = null);
