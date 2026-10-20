using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Administration;
using MediatR;

namespace FatturaFacile.ViewModels;

public sealed partial class LoginViewModel(ISender mediator, ITenantProvider tenantProvider) : ViewModelBase
{
    private readonly ISender _mediator = mediator;
    private readonly ITenantProvider _tenantProvider = tenantProvider;

    [ObservableProperty]
    public partial string Username { get; set; } = "admin";

    [ObservableProperty]
    public partial string Password { get; set; } = "Admin123!";

    [ObservableProperty]
    public partial ObservableCollection<TenantDto> Tenants { get; set; } = [];

    [ObservableProperty]
    public partial TenantDto? SelectedTenant { get; set; }

    public event Action<UserDto>? LoginSuccessful;

    public override async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var tenantList = await _mediator.Send(new GetTenantsQuery());
            Tenants = [with(tenantList)];
            SelectedTenant = Tenants.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (SelectedTenant == null)
        {
            ErrorMessage = "Selezionare un'azienda / Tenant.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var user = await _mediator.Send(new LoginCommand(Username, Password, SelectedTenant.Id));
            LoginSuccessful?.Invoke(user);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
