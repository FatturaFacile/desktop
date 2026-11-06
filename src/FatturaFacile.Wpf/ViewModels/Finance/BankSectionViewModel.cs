// <copyright file="BankSectionViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Banking;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.Features.Banking;
using FatturaFacile.Application.Features.Invoicing.Queries;
using FatturaFacile.Application.Features.Records;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Wpf.Services;
using FatturaFacile.Wpf.ViewModels.Payroll;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels.Finance;

/// <summary>
/// Bank accounts, ISO 20022 statement import and reconciliation with customer and supplier invoices.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BankSectionViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="localizationService">The localization service.</param>
/// <param name="fileDialogService">The file dialogs.</param>
internal sealed partial class BankSectionViewModel(ISender sender, ILocalizationService localizationService, IFileDialogService fileDialogService)
    : PayrollSectionViewModel(sender, localizationService)
{
    [ObservableProperty]
    private BankAccount? _selectedAccount;

    [ObservableProperty]
    private BankAccount _editingAccount = new() { Id = Guid.Empty };

    [ObservableProperty]
    private BankTransaction? _selectedTransaction;

    [ObservableProperty]
    private OpenItemDto? _selectedOpenItem;

    /// <summary>
    /// Gets the bank accounts.
    /// </summary>
    public ObservableCollection<BankAccount> Accounts { get; } = [];

    /// <summary>
    /// Gets the movements of the selected account.
    /// </summary>
    public ObservableCollection<BankTransaction> Transactions { get; } = [];

    /// <summary>
    /// Gets the open customer and supplier invoices for manual reconciliation.
    /// </summary>
    public ObservableCollection<OpenItemDto> OpenItems { get; } = [];

    /// <inheritdoc/>
    [RelayCommand]
    public override Task LoadAsync() => RunAsync(async () =>
    {
        var selectedId = SelectedAccount?.Id;
        var accounts = await Sender.Send(new GetRecordsQuery<BankAccount>());
        Accounts.Clear();

        foreach (var account in accounts.OrderBy(static a => a.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            Accounts.Add(account);
        }

        SelectedAccount = Accounts.FirstOrDefault(a => a.Id == selectedId) ?? Accounts.FirstOrDefault();
        await LoadOpenItemsAsync();
    });

    partial void OnSelectedAccountChanged(BankAccount? value)
    {
        EditingAccount = value is null ? new BankAccount { Id = Guid.Empty } : RecordCopier.Clone(value);
        _ = LoadTransactionsAsync();
    }

    [RelayCommand]
    private void NewAccount()
    {
        SelectedAccount = null;
        EditingAccount = new BankAccount { Id = Guid.Empty };
    }

    [RelayCommand]
    private Task SaveAccountAsync() => RunAsync(
        async () =>
        {
            var id = await Sender.Send(new SaveRecordCommand<BankAccount>(EditingAccount));
            await LoadAsync();
            SelectedAccount = Accounts.FirstOrDefault(a => a.Id == id);
        },
        "Common_Saved");

    [RelayCommand]
    private Task ImportAsync()
    {
        if (SelectedAccount is not { } account)
        {
            return Task.CompletedTask;
        }

        var path = fileDialogService.ShowOpenFile(Localization["Finance_ImportStatement"], "ISO 20022 camt (*.xml)|*.xml|*.*|*.*");

        return path is null
            ? Task.CompletedTask
            : RunAsync(async () =>
            {
                var xml = await File.ReadAllTextAsync(path);
                var result = await Sender.Send(new ImportBankStatementCommand(account.Id, xml));
                await LoadTransactionsAsync();
                await LoadOpenItemsAsync();
                StatusMessage = string.Format(CultureInfo.CurrentCulture, Localization["Finance_Imported"], result.Imported, result.Duplicates, result.Matched);
            });
    }

    [RelayCommand]
    private Task MatchAsync() => SelectedTransaction is null || SelectedOpenItem is null
        ? Task.CompletedTask
        : RunAsync(
            async () =>
            {
                _ = await Sender.Send(new MatchBankTransactionCommand(SelectedTransaction.Id, SelectedOpenItem.DocumentType, SelectedOpenItem.Id));
                await LoadTransactionsAsync();
                await LoadOpenItemsAsync();
            },
            "Finance_Matched");

    [RelayCommand]
    private Task IgnoreAsync()
    {
        if (SelectedTransaction is not { } transaction)
        {
            return Task.CompletedTask;
        }

        transaction.Status = "Ignored";
        return RunAsync(async () =>
        {
            _ = await Sender.Send(new SaveRecordCommand<BankTransaction>(transaction));
            await LoadTransactionsAsync();
        });
    }

    private async Task LoadTransactionsAsync()
    {
        Transactions.Clear();

        if (SelectedAccount is not { } account)
        {
            return;
        }

        var accountId = account.Id;
        var transactions = await Sender.Send(new GetRecordsQuery<BankTransaction>(t => t.BankAccountId == accountId));

        foreach (var transaction in transactions.OrderByDescending(static t => t.BookingDate))
        {
            Transactions.Add(transaction);
        }
    }

    private async Task LoadOpenItemsAsync()
    {
        OpenItems.Clear();

        try
        {
            var invoices = await Sender.Send(new GetInvoicesQuery());

            foreach (var invoice in invoices.Where(static i => !string.Equals(i.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(i.Status, "draft", StringComparison.OrdinalIgnoreCase)))
            {
                OpenItems.Add(new OpenItemDto(BankReconciler.CustomerInvoice, invoice.Id, invoice.InvoiceNumber, invoice.ContactName, invoice.TotalGross));
            }

            var purchases = await Sender.Send(new GetRecordsQuery<PurchaseInvoice>());

            foreach (var purchase in purchases.Where(static p => string.Equals(p.Status, "Posted", StringComparison.Ordinal)))
            {
                OpenItems.Add(new OpenItemDto(BankReconciler.SupplierInvoice, purchase.Id, purchase.DocumentNumber, purchase.PaymentIban ?? string.Empty, -(purchase.TotalGross - purchase.PaidAmount)));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            StatusMessage = ex.Message;
        }
    }
}
