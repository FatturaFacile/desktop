// <copyright file="AutoMapperTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Application.UnitTests;

public class AutoMapperTests
{
    private readonly IMapper mapper;

    public AutoMapperTests()
    {
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddApplication();
        using var serviceProvider = services.BuildServiceProvider();
        mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Mapper_ShouldMap_ClientToClientDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var address = new Address(
            "Via Nassa 12",
            "Lugano",
            "TI",
            "6900",
            "CH");
        var client = Client.CreateSwiss(
            tenantId,
            "Ticino Solutions SA",
            "CHE-109.876.543 MWST",
            "TI",
            new QrIban("CH4431999123456789012"),
            address,
            "info@ticinosolutions.ch",
            "+41 91 123 45 67");

        // Act
        var dto = mapper.Map<ClientDto>(client);

        // Assert
        _ = dto.Should().NotBeNull();
        _ = dto.BusinessName.Should().Be("Ticino Solutions SA");
        _ = dto.Jurisdiction.Should().Be(Jurisdiction.Switzerland);
        _ = dto.CantonalCode.Should().Be("TI");
        _ = dto.TenantId.Should().Be(tenantId);
    }
}
