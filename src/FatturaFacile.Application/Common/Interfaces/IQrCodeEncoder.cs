// <copyright file="IQrCodeEncoder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Models;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Port encoding a text payload into a QR code module matrix (implemented in Infrastructure).
/// </summary>
public interface IQrCodeEncoder
{
    /// <summary>
    /// Encodes the payload as UTF-8 with error correction level M (as required by the Swiss QR-bill).
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <returns>The module matrix without quiet zone.</returns>
    public QrCodeMatrix Encode(string payload);
}
