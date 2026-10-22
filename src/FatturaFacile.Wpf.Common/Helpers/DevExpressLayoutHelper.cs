// <copyright file="DevExpressLayoutHelper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Windows;
using DevExpress.Xpf.Core.Serialization;

namespace FatturaFacile.Wpf.Common.Helpers;

public static class DevExpressLayoutHelper
{
    public static string SaveLayoutToString(DependencyObject rootElement)
    {
        using var stream = new MemoryStream();
        // Serializza l'albero visivo/logico del controllo DevExpress
        DXSerializer.Serialize(rootElement, stream, "FatturaFacileLayout", null);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static void RestoreLayoutFromString(DependencyObject rootElement, string xmlLayout)
    {
        if (string.IsNullOrWhiteSpace(xmlLayout))
        {
            return;
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlLayout));
        DXSerializer.Deserialize(rootElement, stream, "FatturaFacileLayout", null);
    }
}
