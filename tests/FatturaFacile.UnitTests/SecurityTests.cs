// <copyright file="SecurityTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application;
using FatturaFacile.Application.Common.Behaviors;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Features.Invoicing.Commands;
using FatturaFacile.Application.Features.Tenants.Queries;
using FatturaFacile.Application.Features.Users.Commands;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests for users, roles, password hashing and the authorization pipeline.
/// </summary>
public sealed class SecurityTests
{
    /// <summary>
    /// Verifies the PBKDF2 hash round trip, salting and rejection of a wrong password.
    /// </summary>
    [Fact]
    public void PasswordHasher_VerifiesOnlyTheRightPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("correct horse battery staple");

        Assert.StartsWith("PBKDF2-SHA256$600000$", hash, StringComparison.Ordinal);
        Assert.True(hasher.Verify("correct horse battery staple", hash));
        Assert.False(hasher.Verify("Correct horse battery staple", hash));
        Assert.False(string.Equals(hash, hasher.Hash("correct horse battery staple"), StringComparison.Ordinal));
        Assert.False(hasher.Verify("x", "not-a-hash"));
    }

    /// <summary>
    /// Verifies segregation of duties between roles.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <param name="permission">The permission.</param>
    /// <param name="expected">Whether it is granted.</param>
    [Theory]
    [InlineData(UserRole.Viewer, Permission.CommercialRead, true)]
    [InlineData(UserRole.Viewer, Permission.InvoicesWrite, false)]
    [InlineData(UserRole.Sales, Permission.InvoicesIssue, true)]
    [InlineData(UserRole.Sales, Permission.PayrollRead, false)]
    [InlineData(UserRole.Accountant, Permission.AccountingWrite, true)]
    [InlineData(UserRole.Accountant, Permission.PayrollRead, false)]
    [InlineData(UserRole.Payroll, Permission.PayrollWrite, true)]
    [InlineData(UserRole.Payroll, Permission.InvoicesWrite, false)]
    [InlineData(UserRole.Administrator, Permission.UsersManage, true)]
    public void RolePermissions_EnforceSegregationOfDuties(UserRole role, Permission permission, bool expected) =>
        Assert.Equal(expected, RolePermissions.Grants(role, permission));

    /// <summary>
    /// Verifies that the authorization behavior blocks a request without the permission in the current tenant.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task AuthorizationBehavior_BlocksMissingPermission()
    {
        var tenantId = Guid.NewGuid();
        var tenantService = new CurrentTenantService();
        tenantService.SetCurrentTenant(tenantId, "Test", "IT", "EUR");
        var userService = new CurrentUserService();
        userService.SignIn(new AuthenticatedUser(Guid.NewGuid(), "viewer", "Viewer", IsSystemAdministrator: false, new Dictionary<Guid, UserRole> { [tenantId] = UserRole.Viewer }));

        var behavior = new AuthorizationBehavior<IssueInvoiceCommand, bool>(userService, tenantService);

        _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => behavior.Handle(new IssueInvoiceCommand(Guid.NewGuid()), static _ => Task.FromResult(true), CancellationToken.None));

        userService.SignIn(new AuthenticatedUser(Guid.NewGuid(), "sales", "Sales", IsSystemAdministrator: false, new Dictionary<Guid, UserRole> { [tenantId] = UserRole.Sales }));
        Assert.True(await behavior.Handle(new IssueInvoiceCommand(Guid.NewGuid()), static _ => Task.FromResult(true), CancellationToken.None));
    }

    /// <summary>
    /// Verifies the first Windows sign-in bootstraps a system administrator, later unknown accounts are rejected,
    /// and the tenant list is limited to accessible tenants.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task SignInWindowsUser_BootstrapsFirstAdministratorAndRejectsUnknownAccounts()
    {
        var tenantService = new CurrentTenantService();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new TenantSaveChangesInterceptor(tenantService))
            .Options;

        await using var context = new FatturaFacileDbContext(options, tenantService);
        var tenant = new Tenant { Name = "Alpha", LegalName = "Alpha SA", Country = "CH", DefaultCurrency = "CHF" };
        _ = context.Tenants.Add(tenant);
        _ = await context.SaveChangesAsync();

        using var unitOfWork = new UnitOfWork(context);
        var userService = new CurrentUserService();
        var handler = new SignInWindowsUserCommandHandler(unitOfWork, userService, tenantService);

        Assert.True(await handler.Handle(new SignInWindowsUserCommand(@"PC\davide"), CancellationToken.None));
        Assert.True(userService.User!.IsSystemAdministrator);
        Assert.Equal(tenant.Id, tenantService.TenantId);
        Assert.True(userService.HasPermission(tenant.Id, Permission.UsersManage));

        userService.SignOut();
        Assert.False(await handler.Handle(new SignInWindowsUserCommand(@"PC\intruder"), CancellationToken.None));
        Assert.False(userService.IsAuthenticated);

        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddApplicationServices();
        await using var provider = services.BuildServiceProvider();
        var tenantsHandler = new GetTenantsQueryHandler(unitOfWork.Repository<Tenant>(), provider.GetRequiredService<IMapper>(), userService);
        Assert.Empty(await tenantsHandler.Handle(new GetTenantsQuery(), CancellationToken.None));
    }
}
