// <copyright file="FieldServiceMapProjector.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Projects geographic positions onto a canvas without any tile or map library: an equirectangular projection (the longitude is
/// scaled by the cosine of the middle latitude), fitted into the canvas with a padding and the same scale on both axes, north up.
/// </summary>
public static class FieldServiceMapProjector
{
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double Half = 0.5;
    private const double MinSpan = 1e-9;

    /// <summary>
    /// Projects the positions.
    /// </summary>
    /// <param name="points">The positions.</param>
    /// <param name="width">The width of the canvas.</param>
    /// <param name="height">The height of the canvas.</param>
    /// <param name="padding">The margin kept free on every side.</param>
    /// <returns>The canvas positions, in the order of the input; a single position (or equal ones) is centered.</returns>
    public static IReadOnlyList<FieldServiceCanvasPoint> Project(IReadOnlyList<FieldServiceGeoPoint> points, double width, double height, double padding)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count == 0)
        {
            return [];
        }

        var innerWidth = Math.Max(1.0, width - (2 * padding));
        var innerHeight = Math.Max(1.0, height - (2 * padding));
        var minLat = points.Min(point => point.Latitude);
        var maxLat = points.Max(point => point.Latitude);
        var minLng = points.Min(point => point.Longitude);
        var maxLng = points.Max(point => point.Longitude);
        var lngScale = Math.Cos((minLat + maxLat) / 2 * DegreesToRadians);
        var spanX = (maxLng - minLng) * lngScale;
        var spanY = maxLat - minLat;

        var scaleX = spanX < MinSpan ? double.MaxValue : innerWidth / spanX;
        var scaleY = spanY < MinSpan ? double.MaxValue : innerHeight / spanY;
        var scale = spanX < MinSpan && spanY < MinSpan ? 0.0 : Math.Min(scaleX, scaleY);
        var offsetX = padding + ((innerWidth - (spanX * scale)) * Half);
        var offsetY = padding + ((innerHeight - (spanY * scale)) * Half);

        return points
            .Select(point => new FieldServiceCanvasPoint(
                offsetX + ((point.Longitude - minLng) * lngScale * scale),
                offsetY + ((maxLat - point.Latitude) * scale)))
            .ToList();
    }
}
