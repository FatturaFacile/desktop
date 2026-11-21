// <copyright file="FakeKeyProtector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>
#pragma warning disable SA1600 // Elements should be documented: internal test double.

using System.Security.Cryptography;
using FatturaFacile.Application.Security;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Reversible, non-secure protector that lets tests run without the Windows Data Protection API.
/// </summary>
/// <param name="isDpapi">The value reported by <see cref="IsDpapi"/>.</param>
internal sealed class FakeKeyProtector(bool isDpapi = true) : IKeyProtector
{
    private static readonly byte[] _marker = "FAKE"u8.ToArray();

    public string Scheme => "FAKE";

    public bool IsDpapi => isDpapi;

    public byte[] Protect(byte[] clear) => [.. _marker, .. clear.Select(static b => (byte)(b ^ 0x5A))];

    public byte[] Unprotect(byte[] protectedData) =>
        protectedData.Length >= _marker.Length && protectedData.AsSpan(0, _marker.Length).SequenceEqual(_marker)
            ? [.. protectedData.Skip(_marker.Length).Select(static b => (byte)(b ^ 0x5A))]
            : throw new CryptographicException("Not a fake-protected blob.");
}
