// <copyright file="DbInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Seeds initial system reference data, including administrative tenants, exchange rates,
/// sample automotive and invoice records, and core UI translation dictionaries.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Ensures the SQLite database schema is created and seeds initial multi-tenant entities, users, and translations if empty.
    /// </summary>
    /// <param name="context">The database context instance to initialize and populate.</param>
    /// <param name="passwordHasher">The password hashing service used to hash initial administrative user credentials.</param>
    /// <returns>A task representing the asynchronous database initialization operation.</returns>
    public static async Task SeedAsync(
        FatturaFacileDbContext context,
        IPasswordHasher passwordHasher)
    {
        _ = await context.Database.EnsureCreatedAsync();

        // 1. Seed Tenants
        if (!await context.Tenants.AnyAsync())
        {
            var tenantIt = Tenant.Create(
                "TENANT-IT",
                "FatturaFacile Italia S.r.l.",
                "IT",
                Currency.EUR,
                "amministrazione@fatturafacile.it",
                vatNumber: "IT01234567890",
                fiscalCode: "01234567890",
                sdiCode: "M5UXCR1");

            tenantIt.AddBranch(
                "HQ-MILANO",
                "Sede Legale Milano",
                new Address(
                    "Via Monte Napoleone 8",
                    "Milano",
                    "20121",
                    "MI",
                    "IT"),
                isHeadquarters: true);

            var tenantCh = Tenant.Create(
                "TENANT-CH",
                "FatturaFacile Svizzera SA",
                "CH",
                Currency.CHF,
                "info@fatturafacile.ch",
                swissUid: "CHE-123.456.789 MWST",
                qrIban: "CH4431999123000889012");

            tenantCh.AddBranch(
                "HQ-LUGANO",
                "Sede Operativa Lugano",
                new Address(
                    "Via Nassa 12",
                    "Lugano",
                    "6900",
                    "TI",
                    "CH"),
                isHeadquarters: true);

            context.Tenants.AddRange(
                tenantIt,
                tenantCh);
            _ = await context.SaveChangesAsync();

            // 2. Seed Admin User
            passwordHasher.CreateHash(
                "Admin123!",
                out var hash,
                out var salt);
            var adminUser = User.Create(
                tenantIt.Id,
                "admin",
                "admin@fatturafacile.com",
                "Amministratore Sistema",
                hash,
                salt,
                UserRole.SuperAdmin);

            _ = context.Users.Add(adminUser);

            // 3. Seed Exchange Rates
            _ = context.ExchangeRates.Add(
                ExchangeRate.Create(
                    Currency.EUR,
                    Currency.CHF,
                    0.9520m,
                    DateTime.UtcNow,
                    "ECB"));
            _ = context.ExchangeRates.Add(
                ExchangeRate.Create(
                    Currency.CHF,
                    Currency.EUR,
                    1.0504m,
                    DateTime.UtcNow,
                    "SNB"));

            // 4. Seed Cross-Border Clients for Tenant IT
            var clientIt = Client.CreateItalian(
                tenantIt.Id,
                "Rossi Automazioni Industriali S.p.A.",
                new FiscalCode("RSSMRA80A01F205X"),
                new VatNumber(
                    "11223344556",
                    "IT"),
                "SUBM170",
                "rossi.automazioni@pec.it",
                new Address(
                    "Via delle Industrie 45",
                    "Monza",
                    "20900",
                    "MB",
                    "IT"),
                "ordini@rossi-auto.it",
                "+39 039 123456");

            // Seed Cross-Border Client for Tenant CH
            var clientCh = Client.CreateSwiss(
                tenantCh.Id,
                "Gotthard Engineering AG",
                "CHE-987.654.321",
                "TI",
                new QrIban("CH4431999123000889012"),
                new Address(
                    "Via San Gottardo 102",
                    "Bellinzona",
                    "6500",
                    "TI",
                    "CH"),
                "billing@gotthard-eng.ch",
                "+41 91 820 00 00");

            context.Clients.AddRange(
                clientIt,
                clientCh);

            // 5. Seed Automotive Vehicle & Tires
            var vehicle = CustomerVehicle.Create(
                tenantIt.Id,
                clientIt.Id,
                "GE945KZ",
                "WBA3A5C50DF123456",
                "BMW",
                "Serie 3 Touring",
                2023,
                Jurisdiction.Italy,
                38500);

            _ = context.CustomerVehicles.Add(vehicle);

            var tireRecord = TireHotelRecord.CheckIn(
                tenantIt.Id,
                vehicle.Id,
                TireSeason.Winter,
                "Michelin Pilot Alpin 5",
                "225/45 R17 94V",
                "3824",
                7.2m,
                7.1m,
                6.8m,
                6.9m,
                "RACK-B-12-3");

            _ = context.TireHotelRecords.Add(tireRecord);

            // 6. Seed Invoices
            var sampleInvoice = ItalianElectronicInvoice.Create(
                tenantIt.Id,
                "FATT-2026/001",
                DateTime.UtcNow.AddDays(-2),
                clientIt.Id,
                "FatturaFacile Italia S.r.l.",
                "01234567890",
                "01234567890",
                new Address(
                    "Via Monte Napoleone 8",
                    "Milano",
                    "20121",
                    "MI",
                    "IT"),
                "Rossi Automazioni Industriali S.p.A.",
                "11223344556",
                "RSSMRA80A01F205X",
                "SUBM170",
                "rossi.automazioni@pec.it",
                new Address(
                    "Via delle Industrie 45",
                    "Monza",
                    "20900",
                    "MB",
                    "IT"),
                DateTime.UtcNow.AddDays(28));

            sampleInvoice.AddLine(
                "Consulenza e setup sistema ERP industriale",
                1,
                3500.00m,
                22.0m);
            sampleInvoice.AddLine(
                "Licenza modulo Cross-Border IT-CH",
                1,
                1200.00m,
                22.0m);
            _ = context.ItalianElectronicInvoices.Add(sampleInvoice);

            var sampleQrBill = SwissQrBill.Create(
                tenantCh.Id,
                "QR-2026/088",
                DateTime.UtcNow.AddDays(-1),
                clientCh.Id,
                "FatturaFacile Svizzera SA",
                new Address(
                    "Via Nassa 12",
                    "Lugano",
                    "6900",
                    "TI",
                    "CH"),
                new QrIban("CH4431999123000889012"),
                "Gotthard Engineering AG",
                new Address(
                    "Via San Gottardo 102",
                    "Bellinzona",
                    "6500",
                    "TI",
                    "CH"),
                Money.Chf(4850.25m),
                "210000000003139471196009917",
                DateTime.UtcNow.AddDays(30),
                "Incarico perizia tecnica fiduciaria cantone Ticino");

            _ = context.SwissQrBills.Add(sampleQrBill);

            _ = await context.SaveChangesAsync();
        }

        // 7. Seed Localization Strings (it, de, fr, en)
        if (!await context.AppTranslations.AnyAsync())
        {
            var seedKeys = GetStandardTranslations();
            foreach (var (key, (it, de, fr, en)) in seedKeys)
            {
                context.AppTranslations.AddRange(
                    AppTranslation.Create(
                        "it",
                        key,
                        it),
                    AppTranslation.Create(
                        "de",
                        key,
                        de),
                    AppTranslation.Create(
                        "fr",
                        key,
                        fr),
                    AppTranslation.Create(
                        "en",
                        key,
                        en));
            }

            _ = await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, (string It, string De, string Fr, string En)> GetStandardTranslations() =>
        new(StringComparer.Ordinal)
        {
            // Shell & Navigation
            ["App.Title"] = ("FatturaFacile - ERP Transfrontaliero IT/CH",
                "FatturaFacile - Grenzüberschreitendes ERP IT/CH", "FatturaFacile - ERP Transfrontalier IT/CH",
                "FatturaFacile - Cross-Border ERP IT/CH"),
            ["Nav.Dashboard"] = ("Cruscotto", "Dashboard", "Tableau de bord", "Dashboard"),
            ["Nav.Clients"] = ("Clienti & Rubrica", "Kunden & Adressbuch", "Clients & Carnet d'adresses",
                "Clients & Contacts"),
            ["Nav.Invoices"] =
                ("Fatture & QR-Bill", "Rechnungen & QR-Bill", "Factures & QR-Bill", "Invoices & QR-Bill"),
            ["Nav.TireHotel"] = ("Deposito Pneumatici", "Reifenhotel", "Hôtel à pneus", "Tire Storage Hotel"),
            ["Nav.TimeTracking"] = ("Presenze & Timbrature", "Zeiterfassung & Schichten", "Gestion du temps",
                "Time & Attendance"),
            ["Nav.Localization"] = ("Gestione Traduzioni", "Übersetzungsverwaltung", "Gestion traductions",
                "Localization Admin"),
            ["Nav.Settings"] = ("Impostazioni", "Einstellungen", "Paramètres", "Settings"),

            // Top Shell Controls
            ["Header.Tenant"] = ("Azienda / Tenant:", "Mandant / Tenant:", "Entreprise / Tenant:", "Tenant / Company:"),
            ["Header.Language"] = ("Lingua:", "Sprache:", "Langue:", "Language:"),
            ["Header.Theme"] = ("Tema:", "Design:", "Thème:", "Theme:"),
            ["Header.LockSession"] = ("Blocca Sessione", "Sperren", "Verrouiller", "Lock Session"),
            ["Header.Logout"] = ("Disconnetti", "Abmelden", "Déconnexion", "Logout"),

            // Dashboard
            ["Dashboard.Title"] = ("Panoramica Esecutiva Transfrontaliera", "Grenzüberschreitende Führungsübersicht",
                "Aperçu exécutif transfrontalier", "Cross-Border Executive Overview"),
            ["Dashboard.RevenueEur"] = ("Fatturato Italia (EUR)", "Umsatz Italien (EUR)",
                "Chiffre d'affaires Italie (EUR)", "Revenue Italy (EUR)"),
            ["Dashboard.RevenueChf"] = ("Fatturato Svizzera (CHF)", "Umsatz Schweiz (CHF)",
                "Chiffre d'affaires Suisse (CHF)", "Revenue Switzerland (CHF)"),
            ["Dashboard.PendingReceivables"] = ("Crediti da Incassare", "Offene Forderungen", "Créances en suspens",
                "Pending Receivables"),
            ["Dashboard.VatLiability"] = ("Debito IVA Stimato", "Geschätzte MWST-Schuld", "Dette TVA estimée",
                "Estimated VAT Liability"),
            ["Dashboard.NextTaxDeadline"] = ("Prossima Scadenza Fiscale", "Nächste Steuerfrist",
                "Prochaine échéance fiscale", "Next Tax Deadline"),

            // Clients
            ["Clients.Title"] = ("Anagrafica Clienti Transfrontaliera", "Grenzüberschreitende Kundenkartei",
                "Fichier clients transfrontalier", "Cross-Border Client Registry"),
            ["Clients.NewClient"] = ("+ Nuovo Cliente", "+ Neuer Kunde", "+ Nouveau client", "+ New Client"),
            ["Clients.BusinessName"] = ("Ragione Sociale", "Firmenname", "Raison sociale", "Business Name"),
            ["Clients.Jurisdiction"] = ("Giurisdizione", "Rechtsraum", "Juridiction", "Jurisdiction"),
            ["Clients.VatNumber"] = ("Partita IVA / UID", "MWST-Nr. / UID", "N° TVA / IDE", "VAT Number / UID"),
            ["Clients.FiscalCode"] = ("Codice Fiscale", "Steuernummer", "Code fiscal", "Tax / Fiscal Code"),
            ["Clients.SdiCode"] = ("Codice SDI", "SDI-Code", "Code SDI", "SDI Code"),
            ["Clients.QrIban"] = ("QR-IBAN Svizzero", "Schweizer QR-IBAN", "QR-IBAN Suisse", "Swiss QR-IBAN"),
            ["Clients.Address"] = ("Indirizzo", "Adresse", "Adresse", "Address"),

            // Invoices
            ["Invoices.Title"] = ("Fatturazione Elettronica SDI & Swiss QR-Bill", "E-Rechnung SDI & Swiss QR-Bill",
                "Facturation électronique SDI & QR-Bill", "SDI E-Invoicing & Swiss QR-Bill"),
            ["Invoices.GenerateSdiXml"] = ("Genera XML FatturaPA", "FatturaPA XML generieren", "Générer XML FatturaPA",
                "Generate FatturaPA XML"),
            ["Invoices.GenerateQrBill"] = ("Crea Polizza Swiss QR", "Swiss QR-Rechnung erstellen",
                "Créer QR-Facture Suisse", "Create Swiss QR-Bill"),
            ["Invoices.StatusDelivered"] = ("Consegnato SDI", "SDI zugestellt", "Livré SDI", "SDI Delivered"),
            ["Invoices.StatusDraft"] = ("Bozza", "Entwurf", "Brouillon", "Draft"),

            // Tire Hotel
            ["TireHotel.Title"] = ("Modulo Automotive & Hotel Pneumatici", "Automobilmodul & Reifenhotel",
                "Module automobile & Hôtel à pneus", "Automotive & Tire Hotel"),
            ["TireHotel.LicensePlate"] = ("Targa Veicolo", "Kontrollschild / Kennzeichen", "Plaque d'immatriculation",
                "License Plate"),
            ["TireHotel.TreadDepth"] = ("Battistrada (mm)", "Profiltiefe (mm)", "Profondeur sculpture (mm)",
                "Tread Depth (mm)"),
            ["TireHotel.RackLocation"] = ("Posizione Scaffale / Rack", "Regalplatz / Koordinate", "Emplacement étagère",
                "Rack Location"),
            ["TireHotel.Season"] = ("Stagione", "Saison", "Saison", "Season"),

            // Time Tracking
            ["Time.Title"] = ("Rilevazione Presenze (D.Lgs 66/2003 & Svizzera ArG)", "Zeiterfassung (ArG / OLL 1 & IT)",
                "Gestion des temps (ArG & IT)", "Time Tracking (Swiss ArG & IT)"),
            ["Time.ClockIn"] = ("TIMBRA ENTRATA", "KOMMEN", "ENTRÉE", "CLOCK IN"),
            ["Time.ClockOut"] = ("TIMBRA USCITA", "GEHEN", "SORTIE", "CLOCK OUT"),
            ["Time.TotalHours"] = ("Ore Nette Lavorate", "Nettoarbeitszeit", "Heures nettes", "Net Work Hours"),

            // Localization Admin
            ["LocAdmin.Title"] = ("Traduzioni Dinamiche Utente (Persistenza SQLite)",
                "Dynamische Übersetzungen (SQLite)", "Traductions dynamiques (SQLite)",
                "Dynamic User Translations (SQLite)"),
            ["LocAdmin.Search"] = ("Cerca chiave...", "Schlüssel suchen...", "Rechercher clé...", "Search key..."),
            ["LocAdmin.Save"] = ("Salva Modifiche su SQLite", "In SQLite speichern", "Enregistrer dans SQLite",
                "Save Changes to SQLite"),
            ["LocAdmin.CustomValue"] = ("Valore Personalizzato Utente", "Benutzerdefinierter Wert",
                "Valeur personnalisée", "User Custom Value"),
            ["LocAdmin.DefaultValue"] = ("Valore Predefinito di Sistema", "Standardwert", "Valeur par défaut",
                "System Default Value"),

            // General Common
            ["Common.Save"] = ("Salva", "Speichern", "Enregistrer", "Save"),
            ["Common.Cancel"] = ("Annulla", "Abbrechen", "Annuler", "Cancel"),
            ["Common.Search"] = ("Cerca", "Suchen", "Rechercher", "Search"),
            ["Common.Filter"] = ("Filtra", "Filtern", "Filtrer", "Filter"),
            ["Common.Status"] = ("Stato", "Status", "Statut", "Status"),
            ["Common.Total"] = ("Totale", "Total", "Total", "Total"),
        };
}
