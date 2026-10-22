// <copyright file="DevExpressLayoutHelper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Core.Serialization;

namespace FatturaFacile.Wpf.Common.Helpers;

/// <summary>
/// Utility helper for serializing and deserializing DevExpress visual component layouts to XML strings.
/// </summary>
public static class DevExpressLayoutHelper
{
    /// <summary>
    /// Serializes the layout of the specified DevExpress root element into an XML string.
    /// </summary>
    /// <param name="rootElement">The root visual element to serialize.</param>
    /// <returns>An XML string representing the serialized layout.</returns>
    public static string SaveLayoutToString(
        DependencyObject rootElement)
    {
        using var stream = new MemoryStream();
        // Serializza l'albero visivo/logico del controllo DevExpress
        DXSerializer.Serialize(rootElement, stream, "FatturaFacileLayout", null);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Restores the layout of the specified DevExpress root element from an XML string.
    /// </summary>
    /// <param name="rootElement">The root visual element to apply the layout to.</param>
    /// <param name="xmlLayout">The XML string containing serialized layout data.</param>
    public static void RestoreLayoutFromString(
        DependencyObject rootElement,
        string xmlLayout)
    {
        if (string.IsNullOrWhiteSpace(xmlLayout))
        {
            return;
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlLayout));
        DXSerializer.Deserialize(rootElement, stream, "FatturaFacileLayout", null);
    }
}
