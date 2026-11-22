// <copyright file="IReceivablesFileSaver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// Port that saves a downloaded statement on disk.
/// </summary>
public interface IReceivablesFileSaver
{
    /// <summary>
    /// Saves a file in the statements folder of the user.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path of the saved file.</returns>
    public Task<string> SaveAsync(ReceivablesFile file, CancellationToken cancellationToken);
}
