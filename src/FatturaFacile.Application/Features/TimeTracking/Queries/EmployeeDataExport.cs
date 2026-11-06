// <copyright file="EmployeeDataExport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Archive with the personal data of an employee.
/// </summary>
/// <param name="FileName">The suggested file name.</param>
/// <param name="Content">The ZIP archive.</param>
public sealed record EmployeeDataExport(string FileName, ReadOnlyMemory<byte> Content);
