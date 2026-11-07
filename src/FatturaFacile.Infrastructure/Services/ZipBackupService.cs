// <copyright file="ZipBackupService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Data.Sqlite;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Backup to an AES-256 password-protected zip containing a consistent copy of the database (SQLite online backup)
/// and the field encryption key in portable form, so that a backup can be restored on another computer.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ZipBackupService"/> class.
/// </remarks>
/// <param name="paths">The database and key paths.</param>
public sealed class ZipBackupService(DatabasePaths paths) : IBackupService
{
    /// <summary>
    /// The zip entry holding the database.
    /// </summary>
    public const string DatabaseEntry = "fattura-facile.sqlite";

    /// <summary>
    /// The zip entry holding the encryption key.
    /// </summary>
    public const string KeyEntry = "field-encryption.key";

    /// <summary>
    /// The zip folder holding the other files of the data folder (report templates, documents, attachments).
    /// </summary>
    public const string FilesFolder = "files/";

    private const string ManifestEntry = "backup-manifest.json";

    /// <inheritdoc/>
    public Task CreateBackupAsync(
        string            zipPath,
        string            password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zipPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Task.Run(() => WriteBackupArchive(zipPath, password), cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string> RestoreBackupAsync(
        string            zipPath,
        string            password,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zipPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return Task.Run(() => ExtractBackupArchive(zipPath, password), cancellationToken);
    }

    private static void AddEntry(ZipOutputStream zip, string name, byte[] content)
    {
        ZipEntry entry = new(name)
        {
            DateTime = DateTime.Now, AESKeySize = 256, Size = content.Length,
        };

        zip.PutNextEntry(entry);
        zip.Write(content, 0, content.Length);
        zip.CloseEntry();
    }

    private static byte[] ReadEntry(ZipFile zip, string name)
    {
        var entry = zip.GetEntry(name) ??
                    throw new InvalidDataException($"Il file di backup non contiene '{name}'.");

        using var input = zip.GetInputStream(entry);
        using MemoryStream buffer = new();
        input.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static byte[] ReadShared(string path)
    {
        using FileStream stream = new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static Dictionary<string, byte[]> ReadDataFiles(ZipFile zip, string dataDirectory)
    {
        var root = Path.GetFullPath(dataDirectory) + Path.DirectorySeparatorChar;
        Dictionary<string, byte[]> files = [with(StringComparer.OrdinalIgnoreCase)];

        foreach (ZipEntry entry in zip)
        {
            if (!entry.IsFile || !entry.Name.StartsWith(FilesFolder, StringComparison.Ordinal))
            {
                continue;
            }

            var relative = entry.Name[FilesFolder.Length..].Replace('/', Path.DirectorySeparatorChar);
            var target = Path.GetFullPath(Path.Combine(root, relative));

            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Il backup contiene un percorso non valido: '{entry.Name}'.");
            }

            files[target] = ReadEntry(zip, entry.Name);
        }

        return files;
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Lists the files of the data folder to include in the backup: everything except the live database, its journals,
    /// the machine-bound key file, safety copies, temporary files and the backup being written.
    /// </summary>
    /// <param name="zipPath">The backup being written (excluded when it is inside the data folder).</param>
    /// <returns>The zip entry names and file paths.</returns>
    private IEnumerable<(string EntryName, string FilePath)> EnumerateDataFiles(string zipPath)
    {
        var root = paths.DataDirectory;
        var database = Path.GetFullPath(paths.DatabasePath);
        var excluded = new[]
        {
            database, database + "-wal", database + "-shm", database + "-journal",
            Path.GetFullPath(paths.KeyPath), Path.GetFullPath(zipPath),
        };
        var temporary = Path.Combine(root, "tmp") + Path.DirectorySeparatorChar;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var full = Path.GetFullPath(file);

            if (excluded.Contains(full, StringComparer.OrdinalIgnoreCase)      ||
                full.StartsWith(temporary, StringComparison.OrdinalIgnoreCase) || full.EndsWith(
                    ".bak",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relative = Path.GetRelativePath(root, full).Replace(Path.DirectorySeparatorChar, '/');
            yield return (FilesFolder + relative, full);
        }
    }

    [SuppressMessage(
        "Security",
        "SCS0018:Path traversal",
        Justification = "The zip path is chosen by the system administrator in a file dialog.")]
    private void WriteBackupArchive(string zipPath, string password)
    {
        var workDirectory = Path.Combine(Path.GetDirectoryName(paths.DatabasePath) ?? ".", "tmp");
        _ = Directory.CreateDirectory(workDirectory);
        var snapshot = Path.Combine(workDirectory, $"backup-{Guid.NewGuid():N}.sqlite");

        try
        {
            using SqliteConnection source = new($"Data Source={paths.DatabasePath}");

            using SqliteConnection destination = new($"Data Source={snapshot};Pooling=False");

            source.Open();
            destination.Open();
            source.BackupDatabase(destination);

            var key = DpapiKeyStore.LoadOrCreate(paths.KeyPath);
            var manifest = JsonSerializer.SerializeToUtf8Bytes(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["format"] = "1",
                    ["createdUtc"] =
                        DateTime.UtcNow.ToString(
                            "O",
                            System.Globalization.CultureInfo.InvariantCulture),
                    ["machine"] = Environment.MachineName,
                });

            using var output = File.Create(zipPath);
            using ZipOutputStream zip = new(output);
            zip.Password = password;
            zip.SetLevel(9);
            AddEntry(zip, DatabaseEntry, File.ReadAllBytes(snapshot));
            AddEntry(zip, KeyEntry, key);
            AddEntry(zip, ManifestEntry, manifest);

            foreach (var (entryName, filePath) in EnumerateDataFiles(zipPath))
            {
                AddEntry(zip, entryName, ReadShared(filePath));
            }

            zip.Finish();
        }
        finally
        {
            DeleteIfExists(snapshot);
        }
    }

    [SuppressMessage(
        "Security",
        "SCS0018:Path traversal",
        Justification = "The zip path is chosen by the system administrator in a file dialog.")]
    private string ExtractBackupArchive(string zipPath, string password)
    {
        byte[] database;
        byte[] key;
        Dictionary<string, byte[]> dataFiles;

        try
        {
            using ZipFile zip = new(zipPath) { Password = password };
            database = ReadEntry(zip, DatabaseEntry);
            key = ReadEntry(zip, KeyEntry);
            dataFiles = ReadDataFiles(zip, paths.DataDirectory);
        }
        catch (ZipException ex)
        {
            throw new InvalidOperationException(
                "Password errata oppure file di backup danneggiato.",
                ex);
        }

        if (key.Length != 32)
        {
            throw new InvalidDataException("La chiave contenuta nel backup non è valida.");
        }

        if (database.Length < 16 || !database.AsSpan(0, 16).SequenceEqual("SQLite format 3\0"u8))
        {
            throw new InvalidDataException(
                "Il database contenuto nel backup non è un file SQLite valido.");
        }

        SqliteConnection.ClearAllPools();

        var safetyCopy = string.Empty;

        if (File.Exists(paths.DatabasePath))
        {
            safetyCopy = string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"{paths.DatabasePath}.pre-restore-{DateTime.Now:yyyyMMddHHmmss}.bak");
            File.Copy(paths.DatabasePath, safetyCopy, overwrite: true);
        }

        DeleteIfExists(paths.DatabasePath + "-wal");
        DeleteIfExists(paths.DatabasePath + "-shm");
        File.WriteAllBytes(paths.DatabasePath, database);
        DpapiKeyStore.Save(paths.KeyPath, key);

        foreach (var (target, content) in dataFiles)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(target) ?? paths.DataDirectory);
            File.WriteAllBytes(target, content);
        }

        return safetyCopy;
    }
}
