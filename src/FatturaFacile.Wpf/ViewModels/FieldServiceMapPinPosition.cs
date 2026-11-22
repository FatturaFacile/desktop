// <copyright file="FieldServiceMapPinPosition.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The offset of a map pin from the top-left corner of the map.
/// </summary>
/// <param name="Left">The offset from the left edge, in device-independent pixels.</param>
/// <param name="Top">The offset from the top edge, in device-independent pixels.</param>
[StructLayout(LayoutKind.Auto)]
internal readonly record struct FieldServiceMapPinPosition(double Left, double Top);
