// <copyright file="AutoMapperTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Mappings;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FatturaFacile.Tests;

/// <summary>
/// Unit tests verifying AutoMapper profiles, entity-to-DTO mappings, and presentation view model projections.
/// </summary>
public class AutoMapperTests
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMapperTests"/> class and configures AutoMapper profiles.
    /// </summary>
    public AutoMapperTests()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(InvoiceProfile).Assembly, typeof(InvoiceViewModelProfile).Assembly);
        });

        var sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
    }

    /// <summary>
    /// Verifies that the AutoMapper mapper instance resolves and initializes properly.
    /// </summary>
    [Fact]
    public void Configuration_IsValid()
    {
        Assert.NotNull(_mapper);
    }

    /// <summary>
    /// Verifies that Invoice entities map accurately to list item DTOs and presentation row view models.
    /// </summary>
    [Fact]
    public void Invoice_To_InvoiceListItemDto_And_ViewModel_MapsProperly()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            InvoiceNumber = "INV-2026-0001",
            Date = new DateTime(2026, 10, 31),
            DueDate = new DateTime(2026, 11, 30),
            Currency = "CHF",
            TotalNet = 1000m,
            TotalTax = 81m,
            TotalGross = 1081m,
            RoundingAmount = 0m,
            PaymentStatus = PaymentStatus.Paid,
            Contact = new Contact { CompanyName = "Acme Corp" },
        };

        var dto = _mapper.Map<InvoiceListItemDto>(invoice);

        Assert.Equal("INV-2026-0001", dto.InvoiceNumber);
        Assert.Equal("Acme Corp", dto.CustomerName);
        Assert.Equal(1081m, dto.TotalGross);
        Assert.Equal("Paid", dto.PaymentStatus);

        var vm = _mapper.Map<InvoiceRowViewModel>(dto);
        Assert.Contains("1,081.00", vm.FormattedTotalGross.Replace("'", ","));
        Assert.Equal("#2E7D32", vm.StatusBadgeColor);
    }
}
