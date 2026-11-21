// <copyright file="MesTimeSummary.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Accumulated times of a phase in whole seconds. Man time (tempo uomo) is setup plus attended run; machine time (tempo
/// macchina) is setup plus every run; suspensions are downtime and count as neither.
/// </summary>
/// <param name="SetupSeconds">Setup seconds.</param>
/// <param name="AttendedRunSeconds">Seconds of machining with the operator at the machine.</param>
/// <param name="UnattendedRunSeconds">Seconds of automatic machining.</param>
/// <param name="DowntimeSeconds">Seconds of suspension.</param>
public sealed record MesTimeSummary(long SetupSeconds, long AttendedRunSeconds, long UnattendedRunSeconds, long DowntimeSeconds)
{
    /// <summary>
    /// Gets a summary with no time.
    /// </summary>
    public static MesTimeSummary Empty { get; } = new(0, 0, 0, 0);

    /// <summary>
    /// Gets the run seconds, attended or not.
    /// </summary>
    public long RunSeconds => AttendedRunSeconds + UnattendedRunSeconds;

    /// <summary>
    /// Gets the man seconds: setup plus attended run.
    /// </summary>
    public long ManSeconds => SetupSeconds + AttendedRunSeconds;

    /// <summary>
    /// Gets the machine seconds: setup plus every run.
    /// </summary>
    public long MachineSeconds => SetupSeconds + RunSeconds;

    /// <summary>
    /// Adds elapsed seconds to the category of the open interval, so a live counter advances without client-side accumulation:
    /// a suspended phase only gains downtime, an unattended run only gains machine time, a phase with no open interval nothing.
    /// </summary>
    /// <param name="open">The open interval, if any.</param>
    /// <param name="seconds">The elapsed seconds (negative values count as zero).</param>
    /// <returns>The advanced summary.</returns>
    public MesTimeSummary Advance(MesOpenInterval? open, double seconds)
    {
        var extra = (long)Math.Floor(Math.Max(0, seconds));

        return open is null || extra == 0 ? this : Add(open, extra);
    }

    private MesTimeSummary Add(MesOpenInterval open, long extra) => open.Category switch
    {
        MesTimeCategory.Setup => this with { SetupSeconds = SetupSeconds + extra },
        MesTimeCategory.Run when open.MachineAttended => this with { AttendedRunSeconds = AttendedRunSeconds + extra },
        MesTimeCategory.Run => this with { UnattendedRunSeconds = UnattendedRunSeconds + extra },
        MesTimeCategory.Suspension => this with { DowntimeSeconds = DowntimeSeconds + extra },
        _ => this,
    };
}
