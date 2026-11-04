using FluentAssertions;
using NetArchTest.Rules;
using FatturaFacile.Domain.Shared.Abstractions;
using FatturaFacile.Domain.Shared.Errors;
using Xunit;

namespace FatturaFacile.Architecture.Tests;

public class ArchitectureRulesTests
{
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

        result.IsSuccessful.Should().BeTrue("Domain layer must be completely isolated from outer layers.");
    }

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

        result.IsSuccessful.Should().BeTrue("Application layer must not depend on Infrastructure persistence details.");
    }

    [Fact]
    public void EntitiesImplementing_IMustHaveTenant_ShouldResideInDomainAssemblies()
    {
        // Check all types in the entire solution referencing IMustHaveTenant
        var allAssemblies = new[]
        {
            typeof(FatturaFacile.Invoicing.Domain.Aggregates.Invoice).Assembly,
            typeof(FatturaFacile.Crm.Domain.Aggregates.Contact).Assembly,
            typeof(FatturaFacile.Accounting.Domain.Aggregates.GeneralLedgerEntry).Assembly,
            typeof(FatturaFacile.Identity.Domain.Aggregates.User).Assembly
        };

        foreach (var assembly in allAssemblies)
        {
            var tenantEntities = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IMustHaveTenant))
                .GetTypes();

            foreach (var type in tenantEntities)
            {
                type.Namespace.Should().Contain(".Domain.", "All entities implementing IMustHaveTenant must reside exclusively in Domain layers.");
            }
        }
    }

    [Fact]
    public void DomainErrors_ShouldInheritFrom_SharedErrorRecord()
    {
        var errorFields = typeof(DomainErrors.Invoice).GetFields()
            .Concat(typeof(DomainErrors.Identity).GetFields())
            .Concat(typeof(DomainErrors.Tenant).GetFields());

        foreach (var field in errorFields)
        {
            field.FieldType.Should().Be(typeof(DomainErrorDefinition), $"Domain error {field.Name} must be of type DomainErrorDefinition.");
        }
    }
}
