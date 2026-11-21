// <copyright file="IMesOperatorService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Mes;

/// <summary>
/// Offline-first orchestration of the MES operator board: it validates operator commands against the state machine, saves them
/// in the local outbox, sends the outbox to the web-api (with idempotency keys, in the order the operator acted) whenever the
/// server is reachable, and builds the board the screen shows from the server answer or the local cache.
/// </summary>
public interface IMesOperatorService
{
    /// <summary>
    /// Lists the work centres, from the server or the local cache.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The work centres.</returns>
    public Task<IReadOnlyList<MesWorkCenter>> GetWorkCentersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends the outbox, then builds the board of a work centre: from the server when reachable, else from the cache, with the
    /// commands still pending projected onto it.
    /// </summary>
    /// <param name="workCenterId">The work centre.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The board, or null when the server is unreachable and nothing is cached.</returns>
    public Task<MesBoardView?> LoadBoardAsync(Guid workCenterId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a command against the phase it targets, saves it in the outbox and tries to send it at once.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="row">The phase (as projected) the command targets.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>What became of the command.</returns>
    public Task<MesExecuteResult> ExecuteAsync(MesCommand command, MesPhaseRow row, CancellationToken cancellationToken);

    /// <summary>
    /// Sends the commands of the outbox in the order the operator acted.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outcome.</returns>
    public Task<MesFlushResult> FlushAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Dismisses a conflict the operator has seen.
    /// </summary>
    /// <param name="commandId">The command (outbox entry) identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the conflict is dismissed.</returns>
    public Task DismissConflictAsync(Guid commandId, CancellationToken cancellationToken);
}
