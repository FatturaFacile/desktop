// <copyright file="FieldServicePdfResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// How the download of a service report PDF ended.
/// </summary>
/// <param name="Status">The outcome.</param>
/// <param name="Path">The full path of the saved file, or null when nothing was saved.</param>
/// <param name="Detail">The error detail, when there is one.</param>
public sealed record FieldServicePdfResult(FieldServiceCallStatus Status, string? Path, string? Detail);
