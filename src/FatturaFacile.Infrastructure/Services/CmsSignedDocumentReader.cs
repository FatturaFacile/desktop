// <copyright file="CmsSignedDocumentReader.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Buffers;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using FatturaFacile.Application.Common.Interfaces;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Extracts the XML document from CAdES-BES (P7M) envelopes, binary or Base64 encoded. The signature is not verified:
/// the SDI has already checked it before delivering the file.
/// </summary>
public sealed class CmsSignedDocumentReader : ISignedDocumentReader
{
    private static readonly SearchValues<char> Base64Characters =
        SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=\r\n");

    /// <inheritdoc />
    public byte[] Unwrap(ReadOnlyMemory<byte> content)
    {
        var bytes = content.ToArray();

        if (LooksLikeXml(bytes))
        {
            return bytes;
        }

        if (TryDecodeBase64(bytes, out var decoded))
        {
            bytes = decoded;
        }

        try
        {
            SignedCms cms = new();
            cms.Decode(bytes);
            return cms.ContentInfo.Content;
        }
        catch (CryptographicException)
        {
            return bytes;
        }
    }

    private static bool LooksLikeXml(byte[] bytes)
    {
        var start = bytes.AsSpan().StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]) ? 3 : 0;

        while (start < bytes.Length && bytes[start] is (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t')
        {
            start++;
        }

        return start < bytes.Length && bytes[start] == (byte)'<';
    }

    private static bool TryDecodeBase64(byte[] bytes, out byte[] decoded)
    {
        decoded = [];
        var text = Encoding.ASCII.GetString(bytes).Trim();

        if (text.Length == 0 || text.AsSpan().ContainsAnyExcept(Base64Characters))
        {
            return false;
        }

        try
        {
            decoded = Convert.FromBase64String(text);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
