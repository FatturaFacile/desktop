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
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;
    private readonly ILogger<UserStateService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStateService"/> class.
    /// Configures the local storage path under the user's LocalApplicationData folder.
    /// </summary>
    /// <param name="logger">The logger instance used for diagnostics.</param>
    public UserStateService(
        ILogger<UserStateService> logger)
    {
        _logger = logger;

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FatturaFacile");

        _ = Directory.CreateDirectory(appDataFolder);
        _filePath = Path.Combine(appDataFolder, "userstate.json");
    }

    /// <inheritdoc />
    public async ValueTask<UserState> LoadStateAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new UserState();
        }

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var stream = File.OpenRead(_filePath);
            var state = await JsonSerializer.DeserializeAsync<UserState>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            return state ?? new UserState();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Impossibile leggere lo stato utente da {FilePath}. Ritorno stato vuoto.", _filePath);
            return new UserState();
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask SaveStateAsync(
        UserState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(
                stream,
                state,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il salvataggio dello stato utente in {FilePath}", _filePath);
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask ResetStateAsync(
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la rimozione del file di stato utente: {FilePath}", _filePath);
        }
        finally
        {
            _ = _fileLock.Release();
        }
    }
}
