// <copyright file="PlanningCalls.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.ResourcePlanning;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Runs the calls of the resource planning setup screens and turns a refusal, a missing right or an unreachable server into the text to show.
/// </summary>
internal static class PlanningCalls
{
    /// <summary>
    /// Runs a read.
    /// </summary>
    /// <param name="load">The read.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The text of the failure, or null when the read worked.</returns>
    public static Task<string?> ReadAsync(Func<Task> load, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(localization);

        return ReadCoreAsync(load, localization);
    }

    /// <summary>
    /// Runs a write.
    /// </summary>
    /// <typeparam name="T">The value the server returns.</typeparam>
    /// <param name="write">The write.</param>
    /// <param name="format">The formatter that describes a refusal.</param>
    /// <param name="localization">The localization service.</param>
    /// <returns>The outcome with the text to show when it was not accepted.</returns>
    public static Task<PlanningWriteResult<T>> WriteAsync<T>(Func<Task<PlanningCommandOutcome<T>>> write, ResourcePlanningFormatter format, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(format);
        ArgumentNullException.ThrowIfNull(localization);

        return WriteCoreAsync(write, format, localization);
    }

    private static async Task<string?> ReadCoreAsync(Func<Task> load, ILocalizationService localization)
    {
        try
        {
            await load();

            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            return ex.Message;
        }
        catch (HttpRequestException)
        {
            return localization["ResourcePlanning_LoadFailed"];
        }
        catch (JsonException)
        {
            return localization["ResourcePlanning_LoadFailed"];
        }
    }

    private static async Task<PlanningWriteResult<T>> WriteCoreAsync<T>(Func<Task<PlanningCommandOutcome<T>>> write, ResourcePlanningFormatter format, ILocalizationService localization)
    {
        try
        {
            var outcome = await write();

            return outcome.IsSuccess
                ? new PlanningWriteResult<T>(IsSuccess: true, outcome.Value, string.Empty)
                : new PlanningWriteResult<T>(IsSuccess: false, Value: default, format.Refusal(outcome.Status, outcome.Issues));
        }
        catch (UnauthorizedAccessException ex)
        {
            return new PlanningWriteResult<T>(IsSuccess: false, Value: default, ex.Message);
        }
        catch (HttpRequestException)
        {
            return new PlanningWriteResult<T>(IsSuccess: false, Value: default, localization["ResourcePlanning_SaveFailed"]);
        }
        catch (JsonException)
        {
            return new PlanningWriteResult<T>(IsSuccess: false, Value: default, localization["ResourcePlanning_SaveFailed"]);
        }
    }
}
