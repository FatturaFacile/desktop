// <copyright file="HasUsersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Queries;

/// <summary>
/// Handler for <see cref="HasUsersQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HasUsersQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class HasUsersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<HasUsersQuery, bool>
{
    /// <inheritdoc/>
    public async Task<bool> Handle(HasUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Repository<AppUser>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        return users.Count > 0;
    }
}
