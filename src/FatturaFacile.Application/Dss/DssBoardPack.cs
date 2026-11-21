// <copyright file="DssBoardPack.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Dss;

/// <summary>
/// A board pack file received from the server.
/// </summary>
/// <param name="Content">The file bytes.</param>
/// <param name="FileName">The proposed file name.</param>
public sealed record DssBoardPack(ReadOnlyMemory<byte> Content, string FileName);
