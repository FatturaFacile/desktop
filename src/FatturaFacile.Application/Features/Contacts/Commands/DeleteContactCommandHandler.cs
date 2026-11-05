// <copyright file="DeleteContactCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Contacts.Commands;

/// <summary>
/// Handler for <see cref="DeleteContactCommand"/>.
/// </summary>
public sealed class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteContactCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteContactCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var contact = await _unitOfWork.Repository<Contact>().GetByIdAsync(request.ContactId, cancellationToken).ConfigureAwait(false);
        if (contact == null)
        {
            return false;
        }

        _unitOfWork.Repository<Contact>().Delete(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
