// <copyright file="PrivacyAndFilingsTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Application.Common.Export;
using FatturaFacile.Application.Invoicing.Sdi;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Application.Taxation;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Persistence;
using FatturaFacile.Infrastructure.Security;
using FatturaFacile.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of data retention, data export, cantonal holidays, LIPE and SDI files.
/// </summary>
public sealed class PrivacyAndFilingsTests
{
    private const string Receipt =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<types:RicevutaConsegna xmlns:types=\"http://ivaservizi.agenziaentrate.gov.it/docs/xsd/messaggi/v1.0\" versione=\"1.0\">" +
        "<IdentificativoSdI>111</IdentificativoSdI><NomeFile>IT01234567890_00001.xml</NomeFile>" +
        "<DataOraRicezione>2026-11-02T10:00:00.000+01:00</DataOraRicezione><DataOraConsegna>2026-11-02T10:05:00.000+01:00</DataOraConsegna>" +
        "<MessageId>1</MessageId></types:RicevutaConsegna>";

    private const string Rejection =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<types:NotificaScarto xmlns:types=\"http://ivaservizi.agenziaentrate.gov.it/docs/xsd/messaggi/v1.0\" versione=\"1.0\">" +
        "<IdentificativoSdI>112</IdentificativoSdI><NomeFile>IT01234567890_00002.xml</NomeFile>" +
        "<DataOraRicezione>2026-11-02T10:00:00.000+01:00</DataOraRicezione>" +
        "<ListaErrori><Errore><Codice>00404</Codice><Descrizione>Fattura duplicata</Descrizione></Errore></ListaErrori>" +
        "<MessageId>2</MessageId></types:NotificaScarto>";

    private const string SupplierInvoice =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<p:FatturaElettronica versione=\"FPR12\" xmlns:p=\"http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2\">" +
        "<FatturaElettronicaHeader><CedentePrestatore><DatiAnagrafici><IdFiscaleIVA><IdPaese>IT</IdPaese><IdCodice>09876543210</IdCodice></IdFiscaleIVA>" +
        "<Anagrafica><Denominazione>Fornitore Srl</Denominazione></Anagrafica><RegimeFiscale>RF01</RegimeFiscale></DatiAnagrafici>" +
        "<Sede><Indirizzo>Via Roma</Indirizzo><NumeroCivico>1</NumeroCivico><CAP>20100</CAP><Comune>Milano</Comune><Provincia>MI</Provincia><Nazione>IT</Nazione></Sede>" +
        "</CedentePrestatore></FatturaElettronicaHeader>" +
        "<FatturaElettronicaBody><DatiGenerali><DatiGeneraliDocumento><TipoDocumento>TD01</TipoDocumento><Divisa>EUR</Divisa><Data>2026-10-15</Data><Numero>A/77</Numero></DatiGeneraliDocumento></DatiGenerali>" +
        "<DatiBeniServizi><DettaglioLinee><NumeroLinea>1</NumeroLinea><Descrizione>Carta</Descrizione><PrezzoUnitario>100.00</PrezzoUnitario><PrezzoTotale>100.00</PrezzoTotale><AliquotaIVA>22.00</AliquotaIVA></DettaglioLinee>" +
        "<DettaglioLinee><NumeroLinea>2</NumeroLinea><Descrizione>Bollo</Descrizione><PrezzoUnitario>2.00</PrezzoUnitario><PrezzoTotale>2.00</PrezzoTotale><AliquotaIVA>0.00</AliquotaIVA><Natura>N1</Natura></DettaglioLinee></DatiBeniServizi>" +
        "<DatiPagamento><CondizioniPagamento>TP02</CondizioniPagamento><DettaglioPagamento><ModalitaPagamento>MP05</ModalitaPagamento><DataScadenzaPagamento>2026-11-15</DataScadenzaPagamento>" +
        "<ImportoPagamento>124.00</ImportoPagamento><IBAN>IT60X0542811101000000123456</IBAN></DettaglioPagamento></DatiPagamento></FatturaElettronicaBody></p:FatturaElettronica>";

    /// <summary>
    /// The retention keeps at least five full calendar years.
    /// </summary>
    [Fact]
    public void Retention_KeepsAtLeastFiveFullYears()
    {
        Assert.Equal(new DateTime(2021, 1, 1), DataRetentionPolicy.Cutoff(new DateTime(2026, 11, 6), 5));
        Assert.Equal(new DateTime(2021, 1, 1), DataRetentionPolicy.Cutoff(new DateTime(2026, 11, 6), 2));
        Assert.Equal(new DateTime(2016, 1, 1), DataRetentionPolicy.Cutoff(new DateTime(2026, 1, 1), 10));
    }

    /// <summary>
    /// Expired records are deleted and the purge is logged.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task Retention_DeletesExpiredRecordsAndLogsThePurge()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ff-retention-{Guid.NewGuid():N}.sqlite");

        try
        {
            await using var context = CreateContext(path);
            await DatabaseSeeder.SeedAsync(context);
            var tenant = await context.Tenants.FirstAsync();
            var employeeId = Guid.NewGuid();

            _ = context.TimeClockEvents.Add(new TimeClockEvent { TenantId = tenant.Id, EmployeeId = employeeId, Timestamp = new DateTime(2019, 3, 4, 8, 0, 0), EventType = TimeEventType.In });
            _ = context.TimeClockEvents.Add(new TimeClockEvent { TenantId = tenant.Id, EmployeeId = employeeId, Timestamp = new DateTime(2024, 3, 4, 8, 0, 0), EventType = TimeEventType.In });
            _ = context.TimeDailyRecords.Add(new TimeDailyRecord { TenantId = tenant.Id, EmployeeId = employeeId, Date = new DateTime(2020, 12, 31), WorkedMinutes = 480 });
            _ = await context.SaveChangesAsync();

            var result = await new DataRetentionService(context).ApplyAsync(tenant.Id, new DateTime(2026, 11, 6));

            Assert.Equal(1, result.ClockEvents);
            Assert.Equal(1, result.DailyRecords);
            Assert.Equal(1, await context.TimeClockEvents.IgnoreQueryFilters().CountAsync(e => e.EmployeeId == employeeId));
            Assert.Contains(await context.TimeAuditEntries.IgnoreQueryFilters().ToListAsync(), a => string.Equals(a.Action, "RetentionPurge", StringComparison.Ordinal));
            _ = Assert.NotNull((await context.Tenants.IgnoreQueryFilters().FirstAsync(t => t.Id == tenant.Id)).LastRetentionRunAt);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            string[] files = [path, path + "-wal", path + "-shm"];

            foreach (var file in files.Where(File.Exists))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// CSV cells are quoted and formatted invariantly.
    /// </summary>
    [Fact]
    public void Csv_QuotesSeparatorsAndUsesInvariantFormats()
    {
        CsvBuilder csv = new();
        csv.Row("a;b", 1.5m, new DateTime(2026, 1, 2), "x\"y");

        Assert.Equal("\"a;b\";1.5;2026-01-02;\"x\"\"y\"\r\n", csv.ToString());
    }

    /// <summary>
    /// The cantonal holiday lists are applied.
    /// </summary>
    /// <param name="canton">The canton.</param>
    /// <param name="holiday">The holiday.</param>
    /// <param name="expected">The expected.</param>
    [Theory]
    [InlineData("ZH", "San Bertoldo", true)]
    [InlineData("ZH", "Festa del lavoro", true)]
    [InlineData("TI", "Venerdì Santo", false)]
    [InlineData("TI", "SS. Pietro e Paolo", true)]
    [InlineData("GE", "Restaurazione della Repubblica", true)]
    [InlineData("VD", "Lunedì del Digiuno federale", true)]
    [InlineData("VS", "Lunedì di Pasqua", false)]
    [InlineData("BE", "Santo Stefano", true)]
    public void Holidays_FollowTheCantonalLists(string canton, string holiday, bool expected)
    {
        var names = SwissHolidayCalendar.ForCanton(canton, 2026).Select(static h => h.Name);

        Assert.Equal(expected, names.Contains(holiday, StringComparer.Ordinal));
    }

    /// <summary>
    /// Movable cantonal holidays are computed.
    /// </summary>
    [Fact]
    public void Holidays_ComputeMovableDates()
    {
        Assert.Contains(SwissHolidayCalendar.ForCanton("GE", 2026), static h => h.Date == new DateTime(2026, 9, 10));
        Assert.Contains(SwissHolidayCalendar.ForCanton("VD", 2026), static h => h.Date == new DateTime(2026, 9, 21));
        Assert.Contains(SwissHolidayCalendar.ForCanton("GL", 2026), static h => h.Date == new DateTime(2026, 4, 9));
    }

    /// <summary>
    /// The LIPE XML of a quarter is written.
    /// </summary>
    [Fact]
    public void Lipe_WritesTheQuarterlyCommunication()
    {
        LipeData data = new()
        {
            TaxCode = "01234567890",
            VatNumber = "01234567890",
            Year = 2026,
            Quarter = 3,
            ActiveOperations = 10_000m,
            PassiveOperations = 4_000m,
            OutputVat = 2_200m,
            InputVat = 880m,
            Interest = 13.20m,
            Payable = 1_333.20m,
        };

        var xml = Encoding.UTF8.GetString(LipeXmlBuilder.Build(data));

        Assert.Contains("urn:www.agenziaentrate.gov.it:specificheTecniche:sco:ivp", xml, StringComparison.Ordinal);
        Assert.Contains("<iv:Trimestre>3</iv:Trimestre>", xml, StringComparison.Ordinal);
        Assert.Contains("<iv:IvaDovuta>1320,00</iv:IvaDovuta>", xml, StringComparison.Ordinal);
        Assert.Contains("<iv:ImportoDaVersare>1333,20</iv:ImportoDaVersare>", xml, StringComparison.Ordinal);
        Assert.DoesNotContain("IvaCredito", xml, StringComparison.Ordinal);
        Assert.Equal("IT01234567890_LI_26T03.xml", LipeXmlBuilder.FileName(data));
        Assert.Equal("6033", ItalianVatPeriod.F24TaxCode(new DateTime(2026, 7, 1), new DateTime(2026, 9, 30)));
        Assert.Equal("6010", ItalianVatPeriod.F24TaxCode(new DateTime(2026, 10, 1), new DateTime(2026, 10, 31)));
    }

    /// <summary>
    /// SDI receipts and rejections are read.
    /// </summary>
    [Fact]
    public void Sdi_ReadsReceiptsAndRejections()
    {
        Assert.True(SdiNotificationParser.IsNotificationFile("IT01234567890_00001_RC_001.xml"));
        Assert.False(SdiNotificationParser.IsNotificationFile("IT09876543210_A1B2C.xml"));

        var receipt = SdiNotificationParser.Parse(Encoding.UTF8.GetBytes(Receipt));
        Assert.NotNull(receipt);
        Assert.Equal(SdiStatus.Delivered, receipt.ResultingStatus);
        Assert.Equal("IT01234567890_00001.xml", receipt.InvoiceFileName);
        _ = Assert.NotNull(receipt.Timestamp);

        var rejection = SdiNotificationParser.Parse(Encoding.UTF8.GetBytes(Rejection));
        Assert.NotNull(rejection);
        Assert.Equal(SdiStatus.Rejected, rejection.ResultingStatus);
        Assert.Contains("00404", Assert.Single(rejection.Errors), StringComparison.Ordinal);
        Assert.True(SdiStatus.CanSend(SdiStatus.Rejected));
        Assert.False(SdiStatus.CanSend(SdiStatus.Delivered));
    }

    /// <summary>
    /// Received FatturaPA invoices are read.
    /// </summary>
    [Fact]
    public void Sdi_ReadsSupplierInvoices()
    {
        var parsed = Assert.Single(FatturaPaPurchaseParser.Parse(Encoding.UTF8.GetBytes(SupplierInvoice)));

        Assert.Equal("09876543210", parsed.Supplier.VatNumber);
        Assert.Equal("Fornitore Srl", parsed.Supplier.CompanyName);
        Assert.Equal("A/77", parsed.Invoice.DocumentNumber);
        Assert.Equal(new DateTime(2026, 11, 15), parsed.Invoice.DueDate);
        Assert.Equal(2, parsed.Lines.Count);
        Assert.Equal(22m, parsed.Lines[0].VatAmount);
        Assert.Equal("N1", parsed.Lines[1].VatNature);
    }

    /// <summary>
    /// XML and Base64 documents are returned unwrapped.
    /// </summary>
    [Fact]
    public void SignedDocuments_AreUnwrapped()
    {
        CmsSignedDocumentReader reader = new();
        var xml = Encoding.UTF8.GetBytes(SupplierInvoice);

        Assert.Equal(xml, reader.Unwrap(xml));
        Assert.Equal(xml, reader.Unwrap(Encoding.ASCII.GetBytes(Convert.ToBase64String(xml))));
    }

    private static FatturaFacileDbContext CreateContext(string path)
    {
        var tenantService = new CurrentTenantService();
        var options = new DbContextOptionsBuilder<FatturaFacileDbContext>()
            .UseSqlite($"Data Source={path}")
            .AddInterceptors(new TenantSaveChangesInterceptor(tenantService))
            .Options;

        return new FatturaFacileDbContext(options, tenantService, new AesGcmFieldEncryptor(RandomNumberGenerator.GetBytes(32)));
    }
}
