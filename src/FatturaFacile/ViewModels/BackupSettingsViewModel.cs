// <copyright file="BackupSettingsViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Ports;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// View model managing tenant database backups, including online SQLite snapshots, AES-256 encrypted archive exports,
/// and backup history inspection.
/// </summary>
public partial class BackupSettingsViewModel : ViewModelBase
{
    private readonly IBackupService _backupService;

    [ObservableProperty]
    private ObservableCollection<BackupInfo> _backups = new();

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string _exportPassword = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupSettingsViewModel"/> class.
    /// </summary>
    /// <param name="backupService">The database backup service.</param>
    public BackupSettingsViewModel(IBackupService backupService)
    {
        _backupService = backupService;
    }

    /// <inheritdoc />
    public override Task InitializeAsync()
    {
        RefreshBackupList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Scans the local backup repository and updates the observable backup list.
    /// </summary>
    [RelayCommand]
    public void RefreshBackupList()
    {
        Backups.Clear();
        foreach (var b in _backupService.GetAvailableBackups())
        {
            Backups.Add(b);
        }
    }

    /// <summary>
    /// Triggers an immediate point-in-time SQLite snapshot to local application data storage.
    /// </summary>
    [RelayCommand]
    public async Task CreateInstantSnapshotAsync()
    {
        IsBusy = true;
        StatusMessage = "Creating online SQLite snapshot...";
        try
        {
            var res = await _backupService.PerformAutomatedSilentBackupAsync();
            StatusMessage = res.Success ? $"Snapshot saved: {Path.GetFileName(res.FilePath)}" : $"Failed: {res.ErrorMessage}";
            RefreshBackupList();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Exports a password-protected AES-256 encrypted backup archive file to the local backup directory.
    /// </summary>
    [RelayCommand]
    public async Task ExportEncryptedBackupAsync()
    {
        if (string.IsNullOrWhiteSpace(ExportPassword))
        {
            StatusMessage = "Please enter an encryption password.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Encrypting backup with AES-256...";
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var exportFolder = Path.Combine(localAppData, "FatturaFacile", "Backups");
            var exportPath = Path.Combine(exportFolder, $"backup_encrypted_{DateTime.UtcNow:yyyyMMdd_HHmmss}.enc");

            var res = await _backupService.ExportEncryptedBackupAsync(exportPath, ExportPassword);
            StatusMessage = res.Success ? $"Encrypted backup exported: {Path.GetFileName(res.FilePath)}" : $"Export failed: {res.ErrorMessage}";
            RefreshBackupList();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
