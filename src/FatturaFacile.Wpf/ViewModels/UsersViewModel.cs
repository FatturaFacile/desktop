// <copyright file="UsersViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Users.Commands;
using FatturaFacile.Application.Features.Users.Queries;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Manages users, their role in the active company, activation and password resets.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UsersViewModel"/> class.
/// </remarks>
/// <param name="sender">The MediatR sender.</param>
/// <param name="currentTenantService">The active company.</param>
/// <param name="localizationService">The localization service.</param>
internal sealed partial class UsersViewModel(
    ISender sender,
    ICurrentTenantService currentTenantService,
    ILocalizationService localizationService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<UserRow> _users = [];

    [ObservableProperty]
    private UserRow? _selectedUser;

    [ObservableProperty]
    private UserRole _selectedRole = UserRole.Viewer;

    [ObservableProperty]
    private string _newUserName = string.Empty;

    [ObservableProperty]
    private string _newDisplayName = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _newPasswordConfirm = string.Empty;

    [ObservableProperty]
    private bool _newIsSystemAdministrator;

    [ObservableProperty]
    private string _resetPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Gets the available roles.
    /// </summary>
    public IReadOnlyList<UserRole> Roles { get; } = Enum.GetValues<UserRole>();

    /// <summary>
    /// Loads the users.
    /// </summary>
    /// <returns>A task representing the load.</returns>
    [RelayCommand]
    public Task LoadUsersAsync() => RunAsync(ReloadAsync);

    /// <summary>
    /// Creates a user with the selected role in the active company.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task CreateUserAsync()
    {
        return RunAsync(async () =>
        {
            if (!string.Equals(NewPassword, NewPasswordConfirm, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(localizationService["Common_PasswordMismatch"]);
            }

            _ = await sender.Send(new CreateUserCommand
            {
                UserName = NewUserName,
                DisplayName = NewDisplayName,
                Password = NewPassword,
                IsSystemAdministrator = NewIsSystemAdministrator,
                InitialRole = SelectedRole,
            });

            NewUserName = string.Empty;
            NewDisplayName = string.Empty;
            NewPassword = string.Empty;
            NewPasswordConfirm = string.Empty;
            NewIsSystemAdministrator = false;
            await ReloadAsync();
        });
    }

    /// <summary>
    /// Assigns the selected role in the active company to the selected user.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task AssignRoleAsync()
    {
        return RunAsync(async () =>
        {
            if (SelectedUser is null)
            {
                return;
            }

            _ = await sender.Send(new AssignTenantRoleCommand(SelectedUser.Id, currentTenantService.TenantId, SelectedRole));
            await ReloadAsync();
        });
    }

    /// <summary>
    /// Enables or disables the selected user.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task ToggleActiveAsync()
    {
        return RunAsync(async () =>
        {
            if (SelectedUser is null)
            {
                return;
            }

            _ = await sender.Send(new SetUserActiveCommand(SelectedUser.Id, !SelectedUser.IsActive));
            await ReloadAsync();
        });
    }

    /// <summary>
    /// Sets a new password for the selected user.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public Task ResetPasswordAsync()
    {
        return RunAsync(async () =>
        {
            if (SelectedUser is null)
            {
                return;
            }

            _ = await sender.Send(new SetUserPasswordCommand { UserId = SelectedUser.Id, NewPassword = ResetPassword });
            ResetPassword = string.Empty;
        });
    }

    private async Task ReloadAsync()
    {
        var tenantId = currentTenantService.TenantId;
        var users = await sender.Send(new GetUsersQuery());
        Users.Clear();

        foreach (var user in users)
        {
            var role = user.TenantRoles.TryGetValue(tenantId, out var value) ? value : "-";

            if (user.IsSystemAdministrator)
            {
                role = nameof(UserRole.Administrator);
            }

            Users.Add(new UserRow(user.Id, user.UserName, user.DisplayName, role, user.IsActive, user.IsSystemAdministrator));
        }
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            await action();
            StatusMessage = localizationService["Common_Success"];
        }
        catch (Exception ex)
        {
            StatusMessage = $"{localizationService["Common_Error"]}: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
