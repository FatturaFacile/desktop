// <copyright file="UserStateService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Service responsible for asynchronously persisting, loading, and resetting application layout and user state to local app data.
/// </summary>
public sealed class UserStateService : IUserStateService
{

    /// <summary>
    /// The JSON serializer options for reading and writing user state files.
    /// </summary>
    private static readonly JsonSerializerOptions SerializerOptions = new()
                                                                      {
                                                                          WriteIndented               = true,
                                                                          PropertyNameCaseInsensitive = true,
                                                                      };

    /// <summary>
    /// The absolute file path to the user state JSON persistence file.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// The logger used for diagnostics and error reporting.
    /// </summary>
    private readonly ILogger<UserStateService> _logger;

    /// <summary>
    /// The synchronization primitive guarding concurrent file read/write access.
    /// </summary>
    private readonly SemaphoreSlim _fileLock = new(initialCount: 1, maxCount: 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStateService"/> class.
    /// Configures the local storage path under the user's LocalApplicationData folder.
    /// </summary>
    /// <param name="logger">The logger instance used for diagnostics.</param>
    public UserStateService(ILogger<UserStateService> logger)
    {
        _logger = logger;

        var appDataFolder = Path.Combine(
                                         path1: Environment.GetFolderPath(folder: Environment.SpecialFolder.LocalApplicationData),
                                         path2: "FatturaFacile");

        _         = Directory.CreateDirectory(path: appDataFolder);
        _filePath = Path.Combine(path1: appDataFolder, path2: "userstate.json");
    }

    /// <inheritdoc />
    public async ValueTask<UserState> LoadStateAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path: _filePath))
        {
            return new UserState();
        }

        await _fileLock.WaitAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        try
        {
            await using var stream = File.OpenRead(path: _filePath);

            var state = await JsonSerializer.DeserializeAsync<UserState>(
                                                                         utf8Json: stream,
                                                                         options: SerializerOptions,
                                                                         cancellationToken: cancellationToken)
                                            .ConfigureAwait(continueOnCapturedContext: false);

            return state ?? new UserState();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                               exception: ex,
                               message: "Impossibile leggere lo stato utente da {FilePath}. Ritorno stato vuoto.",
                               args: _filePath);

            return new UserState();
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask SaveStateAsync(
        UserState         state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(argument: state);

        await _fileLock.WaitAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        try
        {
            await using var stream = File.Create(path: _filePath);

            await JsonSerializer.SerializeAsync(
                                                utf8Json: stream,
                                                value: state,
                                                options: SerializerOptions,
                                                cancellationToken: cancellationToken)
                                .ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Errore durante il salvataggio dello stato utente in {FilePath}", args: _filePath);
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask ResetStateAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken: cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

        try
        {
            if (File.Exists(path: _filePath))
            {
                File.Delete(path: _filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(exception: ex, message: "Errore durante la rimozione del file di stato utente: {FilePath}", args: _filePath);
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }

}
