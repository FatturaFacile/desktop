// <copyright file="ArchitectureRulesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Errors;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace FatturaFacile.Architecture.Tests;

/// <summary>
/// Architecture compliance tests verifying Clean Architecture layer boundaries and DDD invariants.
/// </summary>
public class ArchitectureRulesTests
{
    /// <summary>
    /// Verifies that domain layers do not reference application, infrastructure, or persistence frameworks.
    /// </summary>
    [Fact]
    public void DomainLayer_ShouldNotDependOn_ApplicationOrInfrastructure()
    {
        var domainAssembly = typeof(FatturaFacile.Invoicing.Domain.Aggregates.Invoice).Assembly;

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "FatturaFacile.Invoicing.Application",
                "FatturaFacile.Invoicing.Infrastructure",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        _ = result.IsSuccessful.Should().BeTrue("Domain layer must be completely isolated from outer layers.");
    }

    /// <summary>
    /// Verifies that application layers do not directly depend on infrastructure persistence implementations.
    /// </summary>
    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_Infrastructure()
    {
        var applicationAssembly = typeof(FatturaFacile.Invoicing.Application.Commands.CreateInvoiceCommand).Assembly;

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "FatturaFacile.Invoicing.Infrastructure",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        _ = result.IsSuccessful.Should().BeTrue("Application layer must not depend on Infrastructure persistence details.");
    }

    /// <summary>
    /// Verifies that entities implementing IMustHaveTenant reside strictly within Domain assemblies.
    /// </summary>
    [Fact]
    public void EntitiesImplementing_IMustHaveTenant_ShouldResideInDomainAssemblies()
    {
        // Check all types in the entire solution referencing IMustHaveTenant
        var allAssemblies = new[]
        {
            typeof(FatturaFacile.Invoicing.Domain.Aggregates.Invoice).Assembly,
            typeof(FatturaFacile.Crm.Domain.Aggregates.Contact).Assembly,
            typeof(FatturaFacile.Accounting.Domain.Aggregates.GeneralLedgerEntry).Assembly,
            typeof(FatturaFacile.Identity.Domain.Aggregates.User).Assembly,
        };

        foreach (var assembly in allAssemblies)
        {
            var tenantEntities = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IMustHaveTenant))
                .GetTypes();

            foreach (var type in tenantEntities)
            {
                _ = type.Namespace.Should().Contain(".Domain.", "All entities implementing IMustHaveTenant must reside exclusively in Domain layers.");
            }
        }
    }

    /// <summary>
    /// Verifies that strongly typed DomainErrors definitions are instances of DomainErrorDefinition.
    /// </summary>
    [Fact]
    public void DomainErrors_ShouldInheritFrom_SharedErrorRecord()
    {
        var errorFields = typeof(DomainErrors.Invoice).GetFields()
            .Concat(typeof(DomainErrors.Identity).GetFields())
            .Concat(typeof(DomainErrors.Tenant).GetFields());

        foreach (var field in errorFields)
        {
            _ = field.FieldType.Should().Be<DomainErrorDefinition>($"Domain error {field.Name} must be of type DomainErrorDefinition.");
        }
    }
}
