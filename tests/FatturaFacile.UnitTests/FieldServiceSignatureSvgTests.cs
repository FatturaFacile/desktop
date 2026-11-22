// <copyright file="FieldServiceSignatureSvgTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;
using FatturaFacile.Application.FieldService;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the stroke to SVG conversion of the signature pad and of the report rules it must satisfy.
/// </summary>
public sealed class FieldServiceSignatureSvgTests
{
    /// <summary>
    /// A stroke becomes a move followed by lines in a single path, on a view box the size of the pad.
    /// </summary>
    [Fact]
    public void Build_DrawsAStrokeAsAPath()
    {
        var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((10, 20), (30.26, 40), (50, 60.04))));

        Assert.Equal(
            "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"300\" height=\"120\" viewBox=\"0 0 300 120\"><path d=\"M 10 20 L 30.3 40 L 50 60\" fill=\"none\" stroke=\"#000000\" stroke-width=\"2.5\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/></svg>",
            svg);
    }

    /// <summary>
    /// Every stroke is its own sub-path, so the pen lifts are kept.
    /// </summary>
    [Fact]
    public void Build_KeepsTheStrokesApart()
    {
        var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((1, 1), (2, 2)), Stroke((5, 5), (6, 6))));

        Assert.Contains("d=\"M 1 1 L 2 2 M 5 5 L 6 6\"", svg, StringComparison.Ordinal);
    }

    /// <summary>
    /// A tap is a dot: a zero length segment that the round cap draws.
    /// </summary>
    [Fact]
    public void Build_DrawsATapAsADot()
    {
        var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((12, 34))));

        Assert.Contains("d=\"M 12 34 L 12 34\"", svg, StringComparison.Ordinal);
    }

    /// <summary>
    /// Nothing drawn, or a pad without a size, gives no signature.
    /// </summary>
    [Fact]
    public void Build_WithoutInkOrSize_ReturnsNull()
    {
        Assert.Null(FieldServiceSignatureSvg.Build(new FieldServiceInkSketch([], 300, 120)));
        Assert.Null(FieldServiceSignatureSvg.Build(new FieldServiceInkSketch([[]], 300, 120)));
        Assert.Null(FieldServiceSignatureSvg.Build(new FieldServiceInkSketch([Stroke((1, 1))], 0, 120)));
        Assert.Null(FieldServiceSignatureSvg.Build(new FieldServiceInkSketch([Stroke((1, 1))], 300, double.NaN)));
    }

    /// <summary>
    /// Points outside the pad are clamped and points that are not numbers are dropped.
    /// </summary>
    [Fact]
    public void Build_ClampsAndDropsBadPoints()
    {
        var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((-5, 500), (double.NaN, 3), (400, -1))));

        Assert.Contains("d=\"M 0 120 L 300 0\"", svg, StringComparison.Ordinal);
    }

    /// <summary>
    /// The numbers use the invariant culture, whatever the culture of the technician is.
    /// </summary>
    [Fact]
    public void Build_UsesTheInvariantCulture()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-CH");

        try
        {
            var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((1.5, 2.5), (3.5, 4.5))));

            Assert.Contains("M 1.5 2.5 L 3.5 4.5", svg, StringComparison.Ordinal);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// The drawing passes the same checks as the server: an svg root, no script, no handler, within the size limit.
    /// </summary>
    [Fact]
    public void Build_PassesTheServerRules()
    {
        var svg = FieldServiceSignatureSvg.Build(Sketch(Stroke((1, 1), (50, 50), (90, 10))));

        Assert.True(FieldServiceValidation.IsSafeSvg(svg));
        Assert.True(Encoding.UTF8.GetByteCount(svg!) <= FieldServiceSignatureSvg.MaxBytes);
    }

    /// <summary>
    /// A drawing with too many points is thinned until it fits the limit of the server, and keeps the ends of the strokes.
    /// </summary>
    [Fact]
    public void Build_ThinsAHugeDrawingToTheLimit()
    {
        var points = Enumerable.Range(0, 60000).Select(index => new FieldServiceInkPoint(index % 300, Math.DivRem(index * 7, 120).Remainder)).ToList();

        var svg = FieldServiceSignatureSvg.Build(Sketch(points));

        Assert.NotNull(svg);
        Assert.True(Encoding.UTF8.GetByteCount(svg) <= FieldServiceSignatureSvg.MaxBytes);
        Assert.True(FieldServiceValidation.IsSafeSvg(svg));
        Assert.Contains("M 0 0", svg, StringComparison.Ordinal);
        Assert.Contains(string.Create(System.Globalization.CultureInfo.InvariantCulture, $"L {points[^1].X} {points[^1].Y}\""), svg, StringComparison.Ordinal);
    }

    /// <summary>
    /// The hash is the lowercase SHA-256 of the UTF-8 text, and changes with the drawing.
    /// </summary>
    [Fact]
    public void Sha256Hex_IsTheKnownDigest()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", FieldServiceSignatureSvg.Sha256Hex("abc"));
        Assert.NotEqual(
            FieldServiceSignatureSvg.Sha256Hex(FieldServiceSignatureSvg.Build(Sketch(Stroke((1, 1), (2, 2))))!),
            FieldServiceSignatureSvg.Sha256Hex(FieldServiceSignatureSvg.Build(Sketch(Stroke((1, 1), (2, 3))))!),
            StringComparer.Ordinal);
    }

    /// <summary>
    /// Only the strokes with at least one valid point count as ink.
    /// </summary>
    [Fact]
    public void CountStrokes_CountsTheStrokesWithInk()
    {
        Assert.Equal(0, FieldServiceSignatureSvg.CountStrokes(sketch: null));
        Assert.Equal(2, FieldServiceSignatureSvg.CountStrokes(Sketch(Stroke((1, 1)), Stroke((double.NaN, 1)), Stroke((2, 2), (3, 3)))));
    }

    /// <summary>
    /// The signer name and role are held to the length limits of the server, and the drawing to its size limit.
    /// </summary>
    [Fact]
    public void CheckReport_HoldsTheSignatureToTheServerLimits()
    {
        var valid = FieldServiceTestData.ReportInput();

        Assert.Null(FieldServiceValidation.CheckReport(valid));
        Assert.Equal(FieldServiceValidation.SignerLengthKey, FieldServiceValidation.CheckReport(valid with { SignerName = new string('a', 151) }));
        Assert.Null(FieldServiceValidation.CheckReport(valid with { SignerName = new string('a', 150), SignerRole = new string('b', 100) }));
        Assert.Equal(FieldServiceValidation.SignerLengthKey, FieldServiceValidation.CheckReport(valid with { SignerRole = new string('b', 101) }));
        Assert.Equal(FieldServiceValidation.SignatureKey, FieldServiceValidation.CheckReport(valid with { SignatureSvg = "<svg>" + new string(' ', FieldServiceSignatureSvg.MaxBytes) + "</svg>" }));
    }

    private static FieldServiceInkSketch Sketch(params IReadOnlyList<FieldServiceInkPoint>[] strokes) => new(strokes, 300, 120);

    private static IReadOnlyList<FieldServiceInkPoint> Stroke(params (double X, double Y)[] points) =>
        [.. points.Select(point => new FieldServiceInkPoint(point.X, point.Y))];
}
