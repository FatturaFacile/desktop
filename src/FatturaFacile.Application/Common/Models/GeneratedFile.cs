// <copyright file="GeneratedFile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Models;

/// <summary>
/// A file produced by the application.
/// </summary>
/// <param name="FileName">The suggested file name.</param>
/// <param name="Content">The content.</param>
/// <param name="Summary">A short description for the user.</param>
public sealed record GeneratedFile(string FileName, ReadOnlyMemory<byte> Content, string Summary);
