// <copyright file="CurrentTenantService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Ports;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// Singleton service holding and managing the currently selected tenant workspace context across the application.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private Guid? _tenantId;
    private TenantDto? _currentTenant;

    /// <inheritdoc />
    public Guid? TenantId
    {
        get => _tenantId;
        set
        {
            if (_tenantId != value)
            {
                _tenantId = value;
                TenantChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <inheritdoc />
    public TenantDto? CurrentTenant
    {
        get => _currentTenant;
        set
        {
            _currentTenant = value;
            if (value?.Id != _tenantId)
            {
                _tenantId = value?.Id;
                TenantChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler? TenantChanged;
}
