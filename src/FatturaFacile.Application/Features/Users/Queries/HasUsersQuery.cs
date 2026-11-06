// <copyright file="HasUsersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Users.Queries;

/// <summary>
/// Returns whether at least one user exists (false on a fresh installation, which then requires the setup wizard).
/// </summary>
public sealed record HasUsersQuery : IRequest<bool>;
