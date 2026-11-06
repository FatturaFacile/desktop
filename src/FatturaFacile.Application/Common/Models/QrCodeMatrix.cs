// <copyright file="QrCodeMatrix.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Models;

/// <summary>
/// Square QR code module matrix without quiet zone (true = dark module).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="QrCodeMatrix"/> class.
/// </remarks>
/// <param name="size">The number of modules per side.</param>
public sealed class QrCodeMatrix(int size)
{
    private readonly bool[] _modules = new bool[size * size];

    /// <summary>
    /// Gets the number of modules per side.
    /// </summary>
    public int Size { get; } = size;

    /// <summary>
    /// Gets a value indicating whether the module is dark.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <returns><see langword="true"/> for a dark module.</returns>
    public bool IsDark(int row, int column) => _modules[(row * Size) + column];

    /// <summary>
    /// Sets the state of a module.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="column">The column index.</param>
    /// <param name="dark">Whether the module is dark.</param>
    public void Set(int row, int column, bool dark) => _modules[(row * Size) + column] = dark;
}
