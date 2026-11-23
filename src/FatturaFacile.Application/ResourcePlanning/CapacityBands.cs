// <copyright file="CapacityBands.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// The saturation rules of the capacity heatmap: load = (hard + soft) / net availability, green below 80 percent, yellow from 80 to
/// 100 percent, red above 100 percent (overload). A load without net capacity but with bookings is capped at 999.
/// </summary>
public static class CapacityBands
{
    /// <summary>
    /// The default load from which a cell is yellow, in percent.
    /// </summary>
    public const double DefaultYellowPercent = 80d;

    /// <summary>
    /// The default load above which a cell is red, in percent.
    /// </summary>
    public const double DefaultRedPercent = 100d;

    /// <summary>
    /// The load shown for bookings against no net capacity, in percent.
    /// </summary>
    public const double CappedLoadPercent = 999d;

    private const int LoadDecimals = 2;
    private const double Percent = 100d;

    /// <summary>
    /// Computes the load of a period.
    /// </summary>
    /// <param name="hardBookedHours">The hours committed.</param>
    /// <param name="softBookedHours">The hours tentatively booked.</param>
    /// <param name="netAvailableHours">The contractual hours minus leave and holidays.</param>
    /// <returns>The load in percent, 0 when nothing is booked and 999 when hours are booked against no capacity.</returns>
    public static double LoadPercent(decimal hardBookedHours, decimal softBookedHours, decimal netAvailableHours)
    {
        var booked = hardBookedHours + softBookedHours;

        return booked <= 0m ? 0d : LoadAgainstCapacity(booked, netAvailableHours);
    }

    /// <summary>
    /// Classifies a load into a band.
    /// </summary>
    /// <param name="loadPercent">The load in percent.</param>
    /// <param name="netAvailableHours">The net availability.</param>
    /// <param name="bookedHours">The hours booked (hard plus soft).</param>
    /// <param name="thresholds">The thresholds of the answer, or null for the defaults.</param>
    /// <returns>The band.</returns>
    public static CapacityBand Classify(double loadPercent, decimal netAvailableHours, decimal bookedHours, HeatmapThresholds? thresholds = null)
    {
        var yellow = thresholds?.Yellow ?? DefaultYellowPercent;
        var red = thresholds?.Red ?? DefaultRedPercent;

        return netAvailableHours <= 0m && bookedHours <= 0m ? CapacityBand.None : ClassifyLoad(loadPercent, yellow, red);
    }

    /// <summary>
    /// Classifies a heatmap cell from the load and hours of the answer.
    /// </summary>
    /// <param name="cell">The cell.</param>
    /// <param name="thresholds">The thresholds of the answer, or null for the defaults.</param>
    /// <returns>The band.</returns>
    public static CapacityBand Classify(HeatmapCell cell, HeatmapThresholds? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(cell);

        return Classify(cell.LoadPercent, cell.NetAvailableHours, cell.HardBookedHours + cell.SoftBookedHours, thresholds);
    }

    /// <summary>
    /// Tells whether a load is an overload.
    /// </summary>
    /// <param name="loadPercent">The load in percent.</param>
    /// <param name="thresholds">The thresholds of the answer, or null for the defaults.</param>
    /// <returns>True above the red threshold.</returns>
    public static bool IsOverload(double loadPercent, HeatmapThresholds? thresholds = null) =>
        loadPercent > (thresholds?.Red ?? DefaultRedPercent);

    private static double LoadAgainstCapacity(decimal booked, decimal netAvailableHours) => netAvailableHours <= 0m
        ? CappedLoadPercent
        : Math.Min(CappedLoadPercent, Math.Round((double)(booked / netAvailableHours) * Percent, LoadDecimals, MidpointRounding.AwayFromZero));

    private static CapacityBand ClassifyLoad(double loadPercent, double yellow, double red) => loadPercent > red ? CapacityBand.Red : ClassifyBelowRed(loadPercent, yellow);

    private static CapacityBand ClassifyBelowRed(double loadPercent, double yellow) => loadPercent >= yellow ? CapacityBand.Yellow : CapacityBand.Green;
}
