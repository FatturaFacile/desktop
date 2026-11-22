// <copyright file="LedgerFakeSender.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Accounting.Commands;
using FatturaFacile.Application.Features.Accounting.Queries;
using FatturaFacile.Domain.Accounting;
using MediatR;

namespace FatturaFacile.UnitTests;

/// <summary>
/// A MediatR sender answering the General Ledger requests from memory and recording every request it receives.
/// </summary>
internal sealed class LedgerFakeSender : ISender
{
    /// <summary>
    /// Gets the cash account of the fake chart.
    /// </summary>
    public static ChartOfAccountDto Cash { get; } = Account(1, "1000", "Cash");

    /// <summary>
    /// Gets the revenue account of the fake chart.
    /// </summary>
    public static ChartOfAccountDto Revenue { get; } = Account(2, "3000", "Revenue");

    /// <summary>
    /// Gets the receivables account of the fake chart.
    /// </summary>
    public static ChartOfAccountDto Receivable { get; } = Account(3, "1100", "Receivables");

    /// <summary>
    /// Gets the output VAT account of the fake chart.
    /// </summary>
    public static ChartOfAccountDto OutputVat { get; } = Account(4, "2200", "Output VAT");

    /// <summary>
    /// Gets the requests received, in order.
    /// </summary>
    public List<object> Requests { get; } = [];

    /// <summary>
    /// Gets or sets the journal the fake returns.
    /// </summary>
    public List<GeneralLedgerEntryDto> Entries { get; set; } = [];

    /// <summary>
    /// Gets or sets the exception the create command raises, when set.
    /// </summary>
    public Exception? CreateFailure { get; set; }

    /// <summary>
    /// Gets or sets the trial balance the fake returns.
    /// </summary>
    public TrialBalanceResult TrialBalance { get; set; } = new(4, [], 0m, 0m, 0m, 0m, IsBalanced: true);

    /// <summary>
    /// Gets or sets the account ledger the fake returns.
    /// </summary>
    public AccountLedgerResult? AccountLedger { get; set; }

    /// <inheritdoc/>
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Requests.Add(request);

        object? answer = request switch
        {
            GetChartOfAccountsQuery => (IReadOnlyList<ChartOfAccountDto>)[Cash, Revenue, Receivable, OutputVat],
            GetAccountingTemplatesQuery => BuiltInTemplates.All("en"),
            GetGeneralLedgerEntriesQuery => Entries,
            PreviewAccountingTemplateQuery preview => TemplateEngine.Preview(preview.Template, preview.Input, [.. Chart().Select(ToInfo)]),
            CreateJournalEntryCommand create => CreateFailure is { } failure ? throw failure : Entry(create.Description, create.LedgerType),
            PostJournalEntryCommand post => Entry("posted", LedgerBook.Statutory, post.EntryId),
            ReverseJournalEntryCommand reverse => Entry("reversal", LedgerBook.Statutory, Guid.NewGuid(), reverse.EntryId),
            GetTrialBalanceQuery => TrialBalance,
            GetAccountLedgerQuery => AccountLedger,
            PullAccountingTemplatesCommand => 3,
            _ => null,
        };

        return Task.FromResult((TResponse)answer!);
    }

    /// <inheritdoc/>
    public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);

    /// <inheritdoc/>
    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest => Task.CompletedTask;

    /// <inheritdoc/>
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private static ChartOfAccountDto Account(int number, string code, string name) => new() { Id = new Guid(number, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), Code = code, Name = name };

    private static ChartOfAccountDto[] Chart() => [Cash, Revenue, Receivable, OutputVat];

    private static LedgerAccountInfo ToInfo(ChartOfAccountDto account) => new(account.Id, account.Code ?? string.Empty, account.Name, account.Type, account.NormalBalance);

    private static GeneralLedgerEntryDto Entry(string description, LedgerBook book, Guid? id = null, Guid? reversalOf = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EntryNumber = "GLE-2026-0001",
        Description = description,
        LedgerType = book,
        IsLocked = true,
        ReversalOfEntryId = reversalOf,
    };
}
