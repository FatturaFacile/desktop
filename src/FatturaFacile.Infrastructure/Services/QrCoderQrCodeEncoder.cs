// <copyright file="QrCoderQrCodeEncoder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using QRCoder;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// QR code encoder based on QRCoder (UTF-8, error correction level M as required by the Swiss QR-bill).
/// </summary>
public sealed class QrCoderQrCodeEncoder : IQrCodeEncoder
{
    /// <inheritdoc/>
    public QrCodeMatrix Encode(string payload)
    {
        ArgumentException.ThrowIfNullOrEmpty(payload);

        using QRCodeGenerator generator = new();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M, forceUtf8: true);

        var matrix = data.ModuleMatrix;
        var size = 21 + (4 * (data.Version - 1));
        var quietZone = (matrix.Count - size) / 2;

        QrCodeMatrix modules = new(size);

        for (var row = 0; row < size; row++)
        {
            for (var col = 0; col < size; col++)
            {
                modules.Set(row, col, matrix[row + quietZone][col + quietZone]);
            }
        }

        return modules;
    }
}
