// <copyright file="PrivacyViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Privacy;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Data protection: register of processing activities and requests of the data subjects (access and erasure).
/// </summary>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class PrivacyViewModel(ISender sender, ILocalizationService localizationService, IFileDialogService fileDialogService)
    : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private ProcessingActivity? _selectedActivity;

    [ObservableProperty]
    private ProcessingActivity _editing = new();

    [ObservableProperty]
    private Contact? _selectedContact;

    /// <summary>
    /// Gets the register of processing activities.
    /// </summary>
    public ObservableCollection<ProcessingActivity> Activities { get; } = [];

    /// <summary>
    /// Gets the customers and suppliers.
    /// </summary>
    public ObservableCollection<Contact> Contacts { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedActivity?.Id;
        Activities.Clear();

        foreach (var activity in (await Sender.Send(new GetRecordsQuery<ProcessingActivity>())).OrderBy(static a => a.Name, StringComparer.CurrentCulture))
        {
            Activities.Add(activity);
        }

        SelectedActivity = Activities.FirstOrDefault(a => a.Id == selectedId) ?? Activities.FirstOrDefault();

        try
        {
            Contacts.Clear();

            foreach (var contact in (await Sender.Send(new GetRecordsQuery<Contact>())).OrderBy(static c => c.DisplayName, StringComparer.CurrentCulture))
            {
                Contacts.Add(contact);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Contacts.Clear();
        }
    });

    partial void OnSelectedActivityChanged(ProcessingActivity? value) => Editing = value is null ? new ProcessingActivity() : RecordCopier.Clone(value);

    [RelayCommand]
    private void New() => SelectedActivity = null;

    [RelayCommand]
    private Task SaveAsync() => RunAsync(
        async () =>
        {
            _ = await Sender.Send(new SaveRecordCommand<ProcessingActivity>(Editing));
            await LoadAsync();
        },
        "Common_Saved");

    [RelayCommand]
    private Task DeleteAsync() => SelectedActivity is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new DeleteRecordCommand<ProcessingActivity>(SelectedActivity.Id));
                await LoadAsync();
            },
            "Common_Deleted");

    [RelayCommand]
    private Task CreateDefaultsAsync() => RunAsync(async () =>
    {
        var count = await Sender.Send(new EnsureProcessingActivitiesCommand());
        await LoadAsync();
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Privacy_DefaultsCreated"], count);
    });

    [RelayCommand]
    private Task ExportRegisterAsync() => RunAsync(async () => await SaveFileAsync(await Sender.Send(new ExportProcessingRegisterQuery()), "CSV (*.csv)|*.csv"));

    [RelayCommand]
    private Task ExportContactAsync() => SelectedContact is null
        ? Task.CompletedTask
        : RunAsync(async () => await SaveFileAsync(await Sender.Send(new ExportContactDataQuery(SelectedContact.Id)), "JSON (*.json)|*.json"));

    [RelayCommand]
    private Task EraseContactAsync() => SelectedContact is null
        ? Task.CompletedTask
        : RunAsync(async () =>
        {
            var message = await Sender.Send(new EraseContactDataCommand(SelectedContact.Id));
            await LoadAsync();
            StatusMessage = message;
        });

    private async Task SaveFileAsync(GeneratedFile file, string filter)
    {
        var path = fileDialogService.ShowSaveFile(Localization["Common_Save"], filter, file.FileName);

        if (path is null)
        {
            return;
        }

        await File.WriteAllBytesAsync(path, file.Content);
        StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Payroll_FileSaved"], path, file.Summary);
    }
}
