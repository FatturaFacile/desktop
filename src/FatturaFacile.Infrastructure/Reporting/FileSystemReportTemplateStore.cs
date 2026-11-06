// <copyright file="FileSystemReportTemplateStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Infrastructure.Persistence;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Report templates stored as .docx files in "Templates\&lt;DocumentType&gt;" inside the data folder.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileSystemReportTemplateStore"/> class.
/// </remarks>
/// <param name="paths">The data paths.</param>
public sealed class FileSystemReportTemplateStore(DatabasePaths paths) : IReportTemplateStore
{
    private const string TemplateExtension = ".docx";

    /// <inheritdoc/>
    public string RootDirectory => Path.Combine(paths.DataDirectory, "Templates");

    /// <inheritdoc/>
    public string GetDirectory(ReportDocumentType documentType)
    {
        var directory = Path.Combine(RootDirectory, documentType.ToString());
        _ = Directory.CreateDirectory(directory);
        return directory;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ReportTemplateInfo> GetTemplates(ReportDocumentType documentType)
    {
        var directory = GetDirectory(documentType);
        return Directory.EnumerateFiles(directory, "*" + TemplateExtension, SearchOption.TopDirectoryOnly)
            .Where(static f => !Path.GetFileName(f).StartsWith("~$", StringComparison.Ordinal))
            .Select(f => new ReportTemplateInfo(Path.GetFileNameWithoutExtension(f), f, documentType, File.GetLastWriteTime(f)))
            .OrderBy(static t => t.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <inheritdoc/>
    public void EnsureDefaultTemplates()
    {
        foreach (var documentType in Enum.GetValues<ReportDocumentType>())
        {
            var directory = GetDirectory(documentType);
            File.WriteAllText(Path.Combine(directory, "_Campi disponibili.txt"), DefaultReportTemplates.FieldReference(documentType));

            if (Directory.EnumerateFiles(directory, "*" + TemplateExtension).Any())
            {
                continue;
            }

            foreach (var (name, content) in DefaultReportTemplates.Create(documentType))
            {
                File.WriteAllBytes(Path.Combine(directory, name + TemplateExtension), content);
            }
        }
    }

    /// <inheritdoc/>
    [SuppressMessage("Security", "SCS0018:Path traversal", Justification = "Names are validated (plain file names) and resolved inside the template folder.")]
    public ReportTemplateInfo Duplicate(ReportDocumentType documentType, string sourceName, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        var source = GetTemplates(documentType).FirstOrDefault(t => string.Equals(t.Name, sourceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Il modello '{sourceName}' non esiste.");
        var directory = GetDirectory(documentType);
        var target = Path.GetFullPath(Path.Combine(directory, newName.Trim() + TemplateExtension));

        if (!string.Equals(Path.GetDirectoryName(target), Path.GetFullPath(directory), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Il nome del modello non è valido.", nameof(newName));
        }

        if (File.Exists(target))
        {
            throw new IOException($"Esiste già un modello chiamato '{newName}'.");
        }

        File.Copy(source.FilePath, target);
        return new ReportTemplateInfo(Path.GetFileNameWithoutExtension(target), target, documentType, File.GetLastWriteTime(target));
    }
}
