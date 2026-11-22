// <copyright file="GeneralLedgerTestDatabase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Repositories;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.UnitTests;

/// <summary>
/// An in-memory SQLite database with the real model, the tenant interceptor (and its ledger guard) and a small chart of accounts,
/// for the General Ledger handler and guard tests.
/// </summary>
internal sealed class GeneralLedgerTestDatabase : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");
    private readonly ServiceProvider provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralLedgerTestDatabase"/> class.
    /// </summary>
    /// <param name="withTriggers">Whether the SQLite guard triggers of the migration are installed.</param>
    public GeneralLedgerTestDatabase(bool withTriggers = false)
    {
        connection.Open();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new TenantSaveChangesInterceptor(Tenant))
            .Options;
        Context = new FatturaFacileDbContext(options, Tenant, new AesGcmFieldEncryptor(new byte[32]));
        _ = Context.Database.EnsureCreated();

        if (withTriggers)
        {
            foreach (var statement in LedgerGuardSql.Create)
            {
                _ = Context.Database.ExecuteSqlRaw(statement);
            }
        }

        ServiceCollection services = new();
        _ = services.AddLogging();
        _ = services.AddApplicationServices();
        provider = services.BuildServiceProvider();
        Mapper = provider.GetRequiredService<IMapper>();
        UnitOfWork = new UnitOfWork(Context);

        Cash = AddAccount("1000", "Cash", "asset", "debit");
        Revenue = AddAccount("3000", "Revenue", "revenue", "credit");
        Expense = AddAccount("4000", "Expense", "expense", "debit");
        _ = Context.SaveChanges();
    }

    /// <summary>
    /// Gets the active tenant service.
    /// </summary>
    public CurrentTenantService Tenant { get; } = new();

    /// <summary>
    /// Gets the database context.
    /// </summary>
    public FatturaFacileDbContext Context { get; }

    /// <summary>
    /// Gets the unit of work over the context.
    /// </summary>
    public UnitOfWork UnitOfWork { get; }

    /// <summary>
    /// Gets the configured mapper.
    /// </summary>
    public IMapper Mapper { get; }

    /// <summary>
    /// Gets the cash account (debit normal).
    /// </summary>
    public ChartOfAccount Cash { get; }

    /// <summary>
    /// Gets the revenue account (credit normal).
    /// </summary>
    public ChartOfAccount Revenue { get; }

    /// <summary>
    /// Gets the expense account (debit normal).
    /// </summary>
    public ChartOfAccount Expense { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        UnitOfWork.Dispose();
        Context.Dispose();
        provider.Dispose();
        connection.Dispose();
    }

    private ChartOfAccount AddAccount(string code, string name, string type, string normal)
    {
        ChartOfAccount account = new()
        {
            TenantId = Tenant.TenantId,
            Code = code,
            AccountCode = code,
            Name = name,
            Type = type,
            AccountType = type,
            NormalBalance = normal,
        };
        _ = Context.Set<ChartOfAccount>().Add(account);
        return account;
    }
}
