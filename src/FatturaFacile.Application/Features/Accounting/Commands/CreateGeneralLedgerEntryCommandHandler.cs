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
public sealed class CreateGeneralLedgerEntryCommandHandler : IRequestHandler<CreateGeneralLedgerEntryCommand, GeneralLedgerEntryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGeneralLedgerEntryCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    public CreateGeneralLedgerEntryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<GeneralLedgerEntryDto> Handle(CreateGeneralLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        GeneralLedgerEntry entry = new()
        {
            TenantId = _currentTenantService.TenantId,
            EntryNumber = request.EntryNumber,
            PostingDate = request.PostingDate,
            DocumentDate = request.DocumentDate,
            Description = request.Description,
            SourceDocumentType = request.SourceDocumentType ?? "manual",
            ReferenceId = request.ReferenceId,
            IsLocked = false,
        };

        var lineNum = 1;
        foreach (var lineDto in request.Lines)
        {
            GeneralLedgerLine line = new()
            {
                GeneralLedgerEntryId = entry.Id,
                AccountId = lineDto.AccountId,
                LineNumber = lineNum++,
                Debit = lineDto.Debit,
                Credit = lineDto.Credit,
                Description = lineDto.Description ?? request.Description,
            };
            entry.Lines.Add(line);
        }

        await _unitOfWork.Repository<GeneralLedgerEntry>().AddAsync(entry, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<GeneralLedgerEntryDto>(entry);
    }
}
