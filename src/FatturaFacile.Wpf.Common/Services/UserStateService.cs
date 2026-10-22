// <copyright file="UserStateService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Wpf.Common.Services;

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

    public UserStateService(ILogger<UserStateService> logger)
    {
        _logger = logger;

        string appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FatturaFacile");

        Directory.CreateDirectory(appDataFolder);
        _filePath = Path.Combine(appDataFolder, "userstate.json");
    }

    public async ValueTask<UserState> LoadStateAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new UserState();
        }

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using FileStream stream = File.OpenRead(_filePath);
            var state = await JsonSerializer.DeserializeAsync<UserState>(
                stream,
                SerializerOptions,
                cancellationToken).ConfigureAwait(false);

            return state ?? new UserState();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossibile leggere lo stato utente da {FilePath}. Ritorno stato vuoto.", _filePath);
            return new UserState();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async ValueTask SaveStateAsync(UserState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        await _fileLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using FileStream stream = File.Create(_filePath);
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
            _fileLock.Release();
        }
    }

    public async ValueTask ResetStateAsync(CancellationToken cancellationToken = default)
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
            _fileLock.Release();
        }
    }
}
