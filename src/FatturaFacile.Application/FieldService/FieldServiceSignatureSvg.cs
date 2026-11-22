// <copyright file="FieldServiceSignatureSvg.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Turns the ink strokes of the signature pad into the vector SVG the server stores and seals, and computes the SHA-256 of the
/// drawing. The output never contains script, event handlers or external references, so it passes
/// <see cref="FieldServiceValidation.IsSafeSvg"/>, and it is thinned until it fits the size limit of the server.
/// </summary>
public static class FieldServiceSignatureSvg
{
    /// <summary>
    /// The largest drawing the server accepts, in bytes (the limit of the signed service report rules).
    /// </summary>
    public const int MaxBytes = 262144;

    private const string CoordinateFormat = "0.#";
    private const string StrokeColor = "#000000";
    private const string StrokeWidth = "2.5";

    /// <summary>
    /// Builds the SVG of a sketch: one path, a sub-path per stroke, with the coordinates clamped to the pad.
    /// </summary>
    /// <param name="sketch">The sketch.</param>
    /// <returns>The SVG, or null when nothing was drawn or the pad has no usable size.</returns>
    public static string? Build(FieldServiceInkSketch sketch)
    {
        ArgumentNullException.ThrowIfNull(sketch);

        if (!double.IsFinite(sketch.Width) || !double.IsFinite(sketch.Height) || sketch.Width < 1 || sketch.Height < 1)
        {
            return null;
        }

        var strokes = Clean(sketch.Strokes);
        if (strokes.Count == 0)
        {
            return null;
        }

        var width = (int)Math.Round(sketch.Width, MidpointRounding.AwayFromZero);
        var height = (int)Math.Round(sketch.Height, MidpointRounding.AwayFromZero);
        var longest = strokes.Max(stroke => stroke.Count);
        var step = 1;
        var svg = Render(strokes, width, height, step);

        while (Encoding.UTF8.GetByteCount(svg) > MaxBytes && step < longest)
        {
            step *= 2;
            svg = Render(strokes, width, height, step);
        }

        return svg;
    }

    /// <summary>
    /// Counts the strokes that carry ink (at least one finite point).
    /// </summary>
    /// <param name="sketch">The sketch, or null.</param>
    /// <returns>The number of strokes.</returns>
    public static int CountStrokes(FieldServiceInkSketch? sketch) => sketch is null ? 0 : Clean(sketch.Strokes).Count;

    /// <summary>
    /// Computes the SHA-256 of a drawing, as lowercase hexadecimal, over its UTF-8 bytes.
    /// </summary>
    /// <param name="svg">The drawing.</param>
    /// <returns>The 64 character hash.</returns>
    public static string Sha256Hex(string svg)
    {
        ArgumentNullException.ThrowIfNull(svg);

        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(svg)));
    }

    private static List<List<FieldServiceInkPoint>> Clean(IReadOnlyList<IReadOnlyList<FieldServiceInkPoint>> strokes)
    {
        List<List<FieldServiceInkPoint>> cleaned = [];
        foreach (var stroke in strokes)
        {
            List<FieldServiceInkPoint> points = [.. stroke.Where(IsUsable)];
            if (points.Count > 0)
            {
                cleaned.Add(points);
            }
        }

        return cleaned;
    }

    private static bool IsUsable(FieldServiceInkPoint? point) => point is not null && double.IsFinite(point.X) && double.IsFinite(point.Y);

    private static string Render(List<List<FieldServiceInkPoint>> strokes, int width, int height, int step)
    {
        var path = new StringBuilder();

        foreach (var stroke in strokes)
        {
            AppendStroke(path, stroke, width, height, step);
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\"><path d=\"{path}\" fill=\"none\" stroke=\"{StrokeColor}\" stroke-width=\"{StrokeWidth}\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>");
    }

    private static void AppendStroke(StringBuilder path, List<FieldServiceInkPoint> stroke, int width, int height, int step)
    {
        for (var index = 0; index < stroke.Count; index += step)
        {
            AppendPoint(path, index == 0 ? 'M' : 'L', stroke[index], width, height);
        }

        if ((stroke.Count - 1) % step != 0)
        {
            AppendPoint(path, 'L', stroke[^1], width, height);
        }

        if (stroke.Count == 1)
        {
            // A tap is a dot: the round line cap draws a zero length segment.
            AppendPoint(path, 'L', stroke[0], width, height);
        }
    }

    private static void AppendPoint(StringBuilder path, char command, FieldServiceInkPoint point, int width, int height)
    {
        if (path.Length > 0)
        {
            _ = path.Append(' ');
        }

        var x = Math.Clamp(point.X, 0, width).ToString(CoordinateFormat, CultureInfo.InvariantCulture);
        var y = Math.Clamp(point.Y, 0, height).ToString(CoordinateFormat, CultureInfo.InvariantCulture);
        _ = path.Append(command).Append(' ').Append(x).Append(' ').Append(y);
    }
}
