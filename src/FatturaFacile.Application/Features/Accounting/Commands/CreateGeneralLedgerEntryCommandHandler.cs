// <copyright file="CreateGeneralLedgerEntryCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Commands;

/// <summary>
/// Handler for <see cref="CreateGeneralLedgerEntryCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="CreateGeneralLedgerEntryCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateGeneralLedgerEntryCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IMapper mapper)
    : IRequestHandler<CreateGeneralLedgerEntryCommand, GeneralLedgerEntryDto>
{
    /// <inheritdoc/>
    public Task<GeneralLedgerEntryDto> Handle(
        CreateGeneralLedgerEntryCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(
            request,
            cancellationToken);
    }

    private async Task<GeneralLedgerEntryDto> HandleInternalAsync(
        CreateGeneralLedgerEntryCommand request,
        CancellationToken cancellationToken)
    {
        GeneralLedgerEntry entry = new()
        {
            TenantId = currentTenantService.TenantId,
            EntryNumber = request.EntryNumber,
            PostingDate = request.PostingDate,
            DocumentDate = request.DocumentDate,
            Description = request.Description,
            SourceDocumentType = request.SourceDocumentType ?? "manual",
            ReferenceId = request.ReferenceId,
            IsLocked = false,
        };

        var lineNumber = 1;

        foreach (var lineDto in request.Lines)
        {
            GeneralLedgerLine line = new()
            {
                GeneralLedgerEntryId = entry.Id,
                AccountId = lineDto.AccountId,
                LineNumber = lineNumber++,
                Debit = lineDto.Debit,
                Credit = lineDto.Credit,
                Description = lineDto.Description ?? request.Description,
            };

            entry.Lines.Add(line);
        }

        await unitOfWork
            .Repository<GeneralLedgerEntry>()
            .AddAsync(entry, cancellationToken)
            .ConfigureAwait(false);

        _ = await unitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return mapper.Map<GeneralLedgerEntryDto>(entry);
    }
}
