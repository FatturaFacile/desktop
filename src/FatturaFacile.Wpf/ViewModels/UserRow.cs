// <copyright file="UserRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A user as shown in the users grid, with the role held in the active company.
/// </summary>
/// <param name="Id">The user identifier.</param>
/// <param name="UserName">The login name.</param>
/// <param name="DisplayName">The display name.</param>
/// <param name="Role">The role in the active company.</param>
/// <param name="IsActive">Whether the user can sign in.</param>
/// <param name="IsSystemAdministrator">Whether the user administers all companies.</param>
internal sealed record UserRow(Guid Id, string UserName, string DisplayName, string Role, bool IsActive, bool IsSystemAdministrator);
