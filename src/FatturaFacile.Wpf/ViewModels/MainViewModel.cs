// <copyright file="MainViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Backup.Commands;
using FatturaFacile.Application.Features.Tenants.Commands;
using FatturaFacile.Application.Features.Tenants.Queries;
using FatturaFacile.Application.Features.Users.Commands;
using FatturaFacile.Application.Features.Users.Queries;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.Services;
using MediatR;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// Root view model for the main application shell: password sign-in, first-run setup, session lock,
/// tenant switching, backup/restore and navigation between ERP modules.
/// </summary>
internal sealed partial class MainViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ILocalizationService _localizationService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IApplicationRestarter _applicationRestarter;

    [ObservableProperty]
    private ObservableCollection<TenantDto> _tenants = [];

    [ObservableProperty]
    private TenantDto? _selectedTenant;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _activeModuleName = string.Empty;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private bool _isLoggedOut = true;

    [ObservableProperty]
    private bool _isSetupRequired;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _lockStatusMessage = string.Empty;

    [ObservableProperty]
    private string _loginStatusMessage = string.Empty;

    [ObservableProperty]
    private string _setupStatusMessage = string.Empty;

    [ObservableProperty]
    private string _shellStatusMessage = string.Empty;

    [ObservableProperty]
    private string _loginUserName = string.Empty;

    [ObservableProperty]
    private string _loginPassword = string.Empty;

    [ObservableProperty]
    private string _unlockPassword = string.Empty;

    [ObservableProperty]
    private string _setupUserName = string.Empty;

    [ObservableProperty]
    private string _setupDisplayName = string.Empty;

    [ObservableProperty]
    private string _setupPassword = string.Empty;

    [ObservableProperty]
    private string _setupPasswordConfirm = string.Empty;

    [ObservableProperty]
    private string _currentUserDisplayName = string.Empty;

    [ObservableProperty]
    private bool _isSystemAdministrator;

    [ObservableProperty]
    private bool _canManageCompany;

    [ObservableProperty]
    private bool _isBackupDialogOpen;

    [ObservableProperty]
    private bool _isRestoreMode;

    [ObservableProperty]
    private string _backupPassword = string.Empty;

    [ObservableProperty]
    private string _backupPasswordConfirm = string.Empty;

    [ObservableProperty]
    private string _backupFilePath = string.Empty;

    [ObservableProperty]
    private string _backupStatusMessage = string.Empty;

    [ObservableProperty]
    private CultureInfo _selectedCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// </summary>
    /// <param name="sender">The MediatR sender.</param>
    /// <param name="currentUserService">The signed-in user session.</param>
    /// <param name="currentTenantService">The active tenant context service.</param>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="fileDialogService">The file dialog service.</param>
    /// <param name="applicationRestarter">The application restarter.</param>
    /// <param name="modules">The module view models.</param>
    public MainViewModel(
        ISender sender,
        ICurrentUserService currentUserService,
        ICurrentTenantService currentTenantService,
        ILocalizationService localizationService,
        IFileDialogService fileDialogService,
        IApplicationRestarter applicationRestarter,
        ShellModules modules)
    {
        ArgumentNullException.ThrowIfNull(modules);

        _sender = sender;
        _currentUserService = currentUserService;
        _currentTenantService = currentTenantService;
        _localizationService = localizationService;
        _fileDialogService = fileDialogService;
        _applicationRestarter = applicationRestarter;
        Modules = modules;

        _currentView = modules.Invoicing;
        _selectedCulture = localizationService.CurrentCulture;
        _localizationService.CultureChanged += (_, _) => UpdateActiveModuleName();
        _currentTenantService.CurrentTenantChanged += (_, _) => OnTenantChanged();
    }

    /// <summary>
    /// Gets the module view models.
    /// </summary>
    public ShellModules Modules { get; }

    /// <summary>
    /// Gets the supported UI cultures.
    /// </summary>
    public IReadOnlyList<CultureInfo> SupportedCultures => _localizationService.SupportedCultures;

    /// <summary>
    /// Determines whether the installation needs the first-run setup or a sign-in.
    /// </summary>
    /// <returns>A task representing the initialization.</returns>
    public async Task InitializeAsync()
    {
        var hasUsers = await _sender.Send(new HasUsersQuery());
        IsSetupRequired = !hasUsers;
        IsLoggedOut = hasUsers;
    }

    /// <summary>
    /// Signs in with user name and password.
    /// </summary>
    /// <returns>A task representing the sign-in.</returns>
    [RelayCommand]
    public async Task LoginAsync()
    {
        LoginStatusMessage = string.Empty;
        var signedIn = await _sender.Send(new SignInWithPasswordCommand { UserName = LoginUserName, Password = LoginPassword });
        LoginPassword = string.Empty;

        if (!signedIn)
        {
            LoginStatusMessage = _localizationService["Login_Failed"];
            return;
        }

        await OnSignedInAsync();
    }

    /// <summary>
    /// Creates the first system administrator and signs it in.
    /// </summary>
    /// <returns>A task representing the setup.</returns>
    [RelayCommand]
    public async Task CompleteSetupAsync()
    {
        SetupStatusMessage = string.Empty;

        if (!string.Equals(SetupPassword, SetupPasswordConfirm, StringComparison.Ordinal))
        {
            SetupStatusMessage = _localizationService["Common_PasswordMismatch"];
            return;
        }

        try
        {
            _ = await _sender.Send(new CreateFirstAdministratorCommand
            {
                UserName = SetupUserName,
                DisplayName = SetupDisplayName,
                Password = SetupPassword,
            });

            SetupPassword = string.Empty;
            SetupPasswordConfirm = string.Empty;
            await OnSignedInAsync();
        }
        catch (Exception ex)
        {
            SetupStatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Navigates to a module.
    /// </summary>
    /// <param name="moduleName">The module name.</param>
    /// <returns>A task representing the navigation.</returns>
    [RelayCommand]
    public async Task NavigateToModuleAsync(string? moduleName)
    {
        try
        {
            CurrentView = moduleName switch
            {
                "Accounting" => Modules.Accounting,
                "Contacts" => Modules.Contacts,
                "Products" => Modules.Products,
                "Payroll" => Modules.Payroll,
                "TimeTracking" => (object?)Modules.TimeTracking ?? Modules.Invoicing,
                "Finance" => (object?)Modules.Finance ?? Modules.Invoicing,
                "Privacy" => (object?)Modules.Privacy ?? Modules.Invoicing,
                "Company" => Modules.Company,
                "Users" => Modules.Users,
                _ => Modules.Invoicing,
            };

            UpdateActiveModuleName();
            await RefreshActiveViewAsync();
        }
        catch (Exception ex)
        {
            ShellStatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Switches the active tenant.
    /// </summary>
    /// <param name="newTenant">The tenant selected by the user.</param>
    /// <returns>A task representing the switch.</returns>
    [RelayCommand]
    public async Task SwitchTenantAsync(TenantDto? newTenant)
    {
        if (newTenant == null || newTenant.Id == _currentTenantService.TenantId)
        {
            return;
        }

        SelectedTenant = newTenant;
        _ = await _sender.Send(new SwitchTenantCommand(newTenant.Id));
    }

    /// <summary>
    /// Locks the session.
    /// </summary>
    [RelayCommand]
    public void LockApplication()
    {
        IsLocked = true;
        LockStatusMessage = string.Empty;
        UnlockPassword = string.Empty;
    }

    /// <summary>
    /// Unlocks the session after verifying the password of the signed-in user.
    /// </summary>
    /// <returns>A task representing the verification.</returns>
    [RelayCommand]
    public async Task UnlockApplicationAsync()
    {
        var userName = _currentUserService.User?.UserName ?? string.Empty;
        var verified = await _sender.Send(new SignInWithPasswordCommand { UserName = userName, Password = UnlockPassword });
        UnlockPassword = string.Empty;

        if (verified)
        {
            IsLocked = false;
            LockStatusMessage = string.Empty;
        }
        else
        {
            LockStatusMessage = _localizationService["Lock_Failed"];
        }
    }

    /// <summary>
    /// Signs the user out.
    /// </summary>
    [RelayCommand]
    public void Logout()
    {
        _currentUserService.SignOut();
        Tenants.Clear();
        SelectedTenant = null;
        CurrentUserDisplayName = string.Empty;
        IsSystemAdministrator = false;
        CanManageCompany = false;
        IsLocked = false;
        IsLoggedOut = true;
        LoginPassword = string.Empty;
        LoginStatusMessage = string.Empty;
    }

    /// <summary>
    /// Opens the backup dialog.
    /// </summary>
    [RelayCommand]
    public void OpenBackupDialog()
    {
        ResetBackupDialog();
        IsRestoreMode = false;
        IsBackupDialogOpen = true;
    }

    /// <summary>
    /// Asks for the backup file and opens the restore dialog.
    /// </summary>
    [RelayCommand]
    public void OpenRestoreDialog()
    {
        var path = _fileDialogService.ShowOpenFile(_localizationService["Restore_Title"], _localizationService["Backup_FileFilter"]);

        if (path is null)
        {
            return;
        }

        ResetBackupDialog();
        BackupFilePath = path;
        IsRestoreMode = true;
        IsBackupDialogOpen = true;
    }

    /// <summary>
    /// Closes the backup/restore dialog.
    /// </summary>
    [RelayCommand]
    public void CancelBackupDialog()
    {
        ResetBackupDialog();
        IsBackupDialogOpen = false;
    }

    /// <summary>
    /// Executes the backup or the restore.
    /// </summary>
    /// <returns>A task representing the operation.</returns>
    [RelayCommand]
    public async Task ConfirmBackupDialogAsync()
    {
        BackupStatusMessage = string.Empty;
        IsBusy = true;

        try
        {
            if (IsRestoreMode)
            {
                _ = await _sender.Send(new RestoreBackupCommand { ZipPath = BackupFilePath, Password = BackupPassword });
                ResetBackupDialog();
                _applicationRestarter.Restart();
                return;
            }

            if (!string.Equals(BackupPassword, BackupPasswordConfirm, StringComparison.Ordinal))
            {
                BackupStatusMessage = _localizationService["Common_PasswordMismatch"];
                return;
            }

            var defaultName = string.Create(CultureInfo.InvariantCulture, $"FatturaFacile-backup-{DateTime.Now:yyyyMMdd-HHmm}.zip");
            var path = _fileDialogService.ShowSaveFile(_localizationService["Backup_Title"], _localizationService["Backup_FileFilter"], defaultName);

            if (path is null)
            {
                return;
            }

            _ = await _sender.Send(new CreateBackupCommand { ZipPath = path, Password = BackupPassword });
            ResetBackupDialog();
            IsBackupDialogOpen = false;
            ShellStatusMessage = $"{_localizationService["Backup_Completed"]} {path}";
        }
        catch (Exception ex)
        {
            BackupStatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedTenantChanged(TenantDto? oldValue, TenantDto? newValue)
    {
        if (newValue != null && oldValue != null && oldValue.Id != newValue.Id)
        {
            _ = SwitchTenantAsync(newValue);
        }
    }

    partial void OnSelectedCultureChanged(CultureInfo? oldValue, CultureInfo newValue)
    {
        if (!Equals(oldValue, newValue))
        {
            _localizationService.SetCulture(newValue);
        }
    }

    private async Task OnSignedInAsync()
    {
        var user = _currentUserService.User;
        CurrentUserDisplayName = user?.DisplayName ?? string.Empty;
        IsSystemAdministrator = user?.IsSystemAdministrator == true;
        UpdatePermissions();

        var tenants = await _sender.Send(new GetTenantsQuery());
        Tenants.Clear();

        foreach (var tenant in tenants)
        {
            Tenants.Add(tenant);
        }

        SelectedTenant = Tenants.FirstOrDefault(t => t.Id == _currentTenantService.TenantId) ?? Tenants.FirstOrDefault();
        IsSetupRequired = false;
        IsLoggedOut = false;
        IsLocked = false;
        LoginStatusMessage = string.Empty;

        await NavigateToModuleAsync("Invoicing");
    }

    private void UpdatePermissions() =>
        CanManageCompany = _currentUserService.HasPermission(_currentTenantService.TenantId, Permission.UsersManage);

    private void ResetBackupDialog()
    {
        BackupPassword = string.Empty;
        BackupPasswordConfirm = string.Empty;
        BackupFilePath = string.Empty;
        BackupStatusMessage = string.Empty;
    }

    private void OnTenantChanged()
    {
        if (SelectedTenant?.Id != _currentTenantService.TenantId)
        {
            SelectedTenant = Tenants.FirstOrDefault(t => t.Id == _currentTenantService.TenantId);
        }

        UpdatePermissions();

        if (!IsLoggedOut)
        {
            _ = RefreshActiveViewAsync();
        }
    }

    private Task RefreshActiveViewAsync()
    {
        return CurrentView switch
        {
            InvoicingViewModel invoicing => invoicing.LoadAllAsync(),
            AccountingViewModel accounting => accounting.LoadDataAsync(),
            ContactsViewModel contacts => contacts.LoadContactsAsync(),
            ProductsViewModel products => products.LoadProductsAsync(),
            PayrollViewModel payroll => payroll.LoadEmployeesAsync(),
            TimeTrackingViewModel timeTracking => timeTracking.LoadAsync(),
            FinanceViewModel finance => finance.LoadAsync(),
            PrivacyViewModel privacy => privacy.LoadAsync(),
            CompanySettingsViewModel company => company.LoadAsync(),
            UsersViewModel users => users.LoadUsersAsync(),
            _ => Task.CompletedTask,
        };
    }

    private void UpdateActiveModuleName()
    {
        var key = CurrentView switch
        {
            AccountingViewModel => "Nav_Accounting",
            ContactsViewModel => "Nav_Contacts",
            ProductsViewModel => "Nav_Products",
            PayrollViewModel => "Nav_Payroll",
            TimeTrackingViewModel => "Nav_TimeTracking",
            FinanceViewModel => "Nav_Finance",
            PrivacyViewModel => "Nav_Privacy",
            CompanySettingsViewModel => "Nav_Company",
            UsersViewModel => "Nav_Users",
            _ => "Nav_Invoicing",
        };

        ActiveModuleName = _localizationService[key];
    }
}
