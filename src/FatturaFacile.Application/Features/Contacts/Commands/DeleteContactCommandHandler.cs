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
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteContactCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class DeleteContactCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteContactCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(
        DeleteContactCommand request,
        CancellationToken cancellationToken)
    {
        var contact = await unitOfWork.Repository<Contact>()
           .GetByIdAsync(request.ContactId, cancellationToken).ConfigureAwait(false);

        if (contact is null)
        {
            return false;
        }

        unitOfWork.Repository<Contact>().Delete(contact);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}
