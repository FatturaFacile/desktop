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
        IPasswordHasher        passwordHasher)
    {
        _ = await context.Database.EnsureCreatedAsync();

        // 1. Seed Tenants
        if (!await context.Tenants.AnyAsync())
        {
            var tenantItData = new TenantData(
                Code: "TENANT-IT",
                BusinessName: "FatturaFacile Italia S.r.l.",
                CountryCode: "IT",
                BaseCurrency: Currency.EUR,
                ContactEmail: "amministrazione@fatturafacile.it",
                VatNumber: "IT01234567890",
                FiscalCode: "01234567890",
                SdiCode: "M5UXCR1");

            var tenantIt = Tenant.Create(data: tenantItData);

            tenantIt.AddBranch(
                               code: "HQ-MILANO",
                               name: "Sede Legale Milano",
                               address: new Address(
                                                    street: "Via Monte Napoleone 8",
                                                    city: "Milano",
                                                    postalCode: "20121",
                                                    provinceOrCanton: "MI",
                                                    countryCode: "IT"),
                               isHeadquarters: true);

            var tenantChData = new TenantData(
                Code: "TENANT-CH",
                BusinessName: "FatturaFacile Svizzera SA",
                CountryCode: "CH",
                BaseCurrency: Currency.CHF,
                ContactEmail: "info@fatturafacile.ch",
                SwissUid: "CHE-123.456.789 MWST",
                QrIban: "CH4431999123000889012");

            var tenantCh = Tenant.Create(data: tenantChData);

            tenantCh.AddBranch(
                               code: "HQ-LUGANO",
                               name: "Sede Operativa Lugano",
                               address: new Address(
                                                    street: "Via Nassa 12",
                                                    city: "Lugano",
                                                    postalCode: "6900",
                                                    provinceOrCanton: "TI",
                                                    countryCode: "CH"),
                               isHeadquarters: true);

            context.Tenants.AddRange(
                                     entities: [tenantIt, tenantCh]);

            _ = await context.SaveChangesAsync();

            // 2. Seed Admin User
            passwordHasher.CreateHash(
                                      password: "Admin123!",
                                      hash: out var hash,
                                      salt: out var salt);

            var adminUser = User.Create(
                                        tenantId: tenantIt.Id,
                                        username: "admin",
                                        email: "admin@fatturafacile.com",
                                        fullName: "Amministratore Sistema",
                                        passwordHash: hash,
                                        salt: salt,
                                        role: UserRole.SuperAdmin);

            _ = context.Users.Add(entity: adminUser);

            // 3. Seed Exchange Rates
            _ = context.ExchangeRates.Add(
                                          entity: ExchangeRate.Create(
                                                                      baseCurr: Currency.EUR,
                                                                      targetCurr: Currency.CHF,
                                                                      rate: 0.9520m,
                                                                      date: DateTime.UtcNow,
                                                                      source: "ECB"));

            _ = context.ExchangeRates.Add(
                                          entity: ExchangeRate.Create(
                                                                      baseCurr: Currency.CHF,
                                                                      targetCurr: Currency.EUR,
                                                                      rate: 1.0504m,
                                                                      date: DateTime.UtcNow,
                                                                      source: "SNB"));

            // 4. Seed Cross-Border Clients for Tenant IT
            var clientItData = new ItalianClientData(
                TenantId: tenantIt.Id,
                BusinessName: "Rossi Automazioni Industriali S.p.A.",
                FiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                VatNumber: new VatNumber(
                    value: "11223344556",
                    countryCode: "IT"),
                SdiCode: "SUBM170",
                PecEmail: "rossi.automazioni@pec.it",
                Address: new Address(
                    street: "Via delle Industrie 45",
                    city: "Monza",
                    postalCode: "20900",
                    provinceOrCanton: "MB",
                    countryCode: "IT"),
                Email: "ordini@rossi-auto.it",
                Phone: "+39 039 123456");

            var clientIt = Client.CreateItalian(data: clientItData);

            // Seed Cross-Border Client for Tenant CH
            var clientChData = new SwissClientData(
                TenantId: tenantCh.Id,
                BusinessName: "Gotthard Engineering AG",
                SwissUid: "CHE-987.654.321",
                CantonalCode: "TI",
                QrIban: new QrIban(value: "CH4431999123000889012"),
                Address: new Address(
                    street: "Via San Gottardo 102",
                    city: "Bellinzona",
                    postalCode: "6500",
                    provinceOrCanton: "TI",
                    countryCode: "CH"),
                Email: "billing@gotthard-eng.ch",
                Phone: "+41 91 820 00 00");

            var clientCh = Client.CreateSwiss(data: clientChData);

            context.Clients.AddRange(
                                     entities: [clientIt, clientCh]);

            // 5. Seed Automotive Vehicle & Tires
            var vehicleData = new CustomerVehicleData(
                TenantId: tenantIt.Id,
                ClientId: clientIt.Id,
                LicensePlate: "GE945KZ",
                Vin: "WBA3A5C50DF123456",
                Brand: "BMW",
                Model: "Serie 3 Touring",
                Year: 2023,
                Jurisdiction: Jurisdiction.Italy,
                Km: 38500);

            var vehicle = CustomerVehicle.Create(data: vehicleData);

            _ = context.CustomerVehicles.Add(entity: vehicle);

            var tireData = new TireHotelCheckInData(
                TenantId: tenantIt.Id,
                VehicleId: vehicle.Id,
                Season: TireSeason.Winter,
                Brand: "Michelin Pilot Alpin 5",
                Dimension: "225/45 R17 94V",
                DotCode: "3824",
                FlMm: 7.2m,
                FrMm: 7.1m,
                RlMm: 6.8m,
                RrMm: 6.9m,
                RackCoordinate: "RACK-B-12-3");

            var tireRecord = TireHotelRecord.CheckIn(data: tireData);

            _ = context.TireHotelRecords.Add(entity: tireRecord);

            // 6. Seed Invoices
            var sampleInvoice = ItalianElectronicInvoice.Create(
                                                                tenantId: tenantIt.Id,
                                                                documentNumber: "FATT-2026/001",
                                                                documentDate: DateTime.UtcNow.AddDays(value: -2),
                                                                clientId: clientIt.Id,
                                                                cedenteName: "FatturaFacile Italia S.r.l.",
                                                                cedenteVat: "01234567890",
                                                                cedenteCf: "01234567890",
                                                                cedenteAddress: new Address(
                                                                                            street: "Via Monte Napoleone 8",
                                                                                            city: "Milano",
                                                                                            postalCode: "20121",
                                                                                            provinceOrCanton: "MI",
                                                                                            countryCode: "IT"),
                                                                cessionarioName: "Rossi Automazioni Industriali S.p.A.",
                                                                cessionarioVat: "11223344556",
                                                                cessionarioCf: "RSSMRA80A01F205X",
                                                                sdiCode: "SUBM170",
                                                                pec: "rossi.automazioni@pec.it",
                                                                cessionarioAddress: new Address(
                                                                                                street: "Via delle Industrie 45",
                                                                                                city: "Monza",
                                                                                                postalCode: "20900",
                                                                                                provinceOrCanton: "MB",
                                                                                                countryCode: "IT"),
                                                                dueDate: DateTime.UtcNow.AddDays(value: 28));

            sampleInvoice.AddLine(
                                  description: "Consulenza e setup sistema ERP industriale",
                                  quantity: 1,
                                  unitPrice: 3500.00m,
                                  vatRate: 22.0m);

            sampleInvoice.AddLine(
                                  description: "Licenza modulo Cross-Border IT-CH",
                                  quantity: 1,
                                  unitPrice: 1200.00m,
                                  vatRate: 22.0m);

            _ = context.ItalianElectronicInvoices.Add(entity: sampleInvoice);

            var qrBillData = new SwissQrBillData(
                TenantId: tenantCh.Id,
                BillNumber: "QR-2026/088",
                BillDate: DateTime.UtcNow.AddDays(value: -1),
                ClientId: clientCh.Id,
                CreditorName: "FatturaFacile Svizzera SA",
                CreditorAddress: new Address(
                    street: "Via Nassa 12",
                    city: "Lugano",
                    postalCode: "6900",
                    provinceOrCanton: "TI",
                    countryCode: "CH"),
                CreditorQrIban: new QrIban(value: "CH4431999123000889012"),
                DebtorName: "Gotthard Engineering AG",
                DebtorAddress: new Address(
                    street: "Via San Gottardo 102",
                    city: "Bellinzona",
                    postalCode: "6500",
                    provinceOrCanton: "TI",
                    countryCode: "CH"),
                RawAmount: Money.Chf(amount: 4850.25m),
                ReferenceNumber: "210000000003139471196009917",
                DueDate: DateTime.UtcNow.AddDays(value: 30),
                Message: "Incarico perizia tecnica fiduciaria cantone Ticino");

            var sampleQrBill = SwissQrBill.Create(data: qrBillData);

            _ = context.SwissQrBills.Add(entity: sampleQrBill);

            _ = await context.SaveChangesAsync();
        }

        // 7. Seed Localization Strings (it, de, fr, en)
        if (!await context.AppTranslations.AnyAsync())
        {
            var seedKeys = GetStandardTranslations();

            foreach (var (key, (it, de, fr, en)) in seedKeys)
            {
                context.AppTranslations.AddRange(
                                                 entities:
                                                 [
                                                     AppTranslation.Create(
                                                                           locale: "it",
                                                                           key: key,
                                                                           defaultValue: it),
                                                     AppTranslation.Create(
                                                                           locale: "de",
                                                                           key: key,
                                                                           defaultValue: de),
                                                     AppTranslation.Create(
                                                                           locale: "fr",
                                                                           key: key,
                                                                           defaultValue: fr),
                                                     AppTranslation.Create(
                                                                           locale: "en",
                                                                           key: key,
                                                                           defaultValue: en),
                                                 ]);
            }

            _ = await context.SaveChangesAsync();
        }
    }

    private static Dictionary<string, (string It, string De, string Fr, string En)> GetStandardTranslations() =>
        new(comparer: StringComparer.Ordinal)
        {
            // Shell & Navigation
            [key: "App.Title"] = ("FatturaFacile - ERP Transfrontaliero IT/CH",
                                  "FatturaFacile - Grenzüberschreitendes ERP IT/CH",
                                  "FatturaFacile - ERP Transfrontalier IT/CH",
                                  "FatturaFacile - Cross-Border ERP IT/CH"),
            [key: "Nav.Dashboard"] = ("Cruscotto", "Dashboard", "Tableau de bord", "Dashboard"),
            [key: "Nav.Clients"] = ("Clienti & Rubrica", "Kunden & Adressbuch", "Clients & Carnet d'adresses",
                                    "Clients & Contacts"),
            [key: "Nav.Invoices"] =
                ("Fatture & QR-Bill", "Rechnungen & QR-Bill", "Factures & QR-Bill", "Invoices & QR-Bill"),
            [key: "Nav.TireHotel"] = ("Deposito Pneumatici", "Reifenhotel", "Hôtel à pneus", "Tire Storage Hotel"),
            [key: "Nav.TimeTracking"] = ("Presenze & Timbrature", "Zeiterfassung & Schichten", "Gestion du temps",
                                         "Time & Attendance"),
            [key: "Nav.Localization"] = ("Gestione Traduzioni", "Übersetzungsverwaltung", "Gestion traductions",
                                         "Localization Admin"),
            [key: "Nav.Settings"] = ("Impostazioni", "Einstellungen", "Paramètres", "Settings"),

            // Top Shell Controls
            [key: "Header.Tenant"]      = ("Azienda / Tenant:", "Mandant / Tenant:", "Entreprise / Tenant:", "Tenant / Company:"),
            [key: "Header.Language"]    = ("Lingua:", "Sprache:", "Langue:", "Language:"),
            [key: "Header.Theme"]       = ("Tema:", "Design:", "Thème:", "Theme:"),
            [key: "Header.LockSession"] = ("Blocca Sessione", "Sperren", "Verrouiller", "Lock Session"),
            [key: "Header.Logout"]      = ("Disconnetti", "Abmelden", "Déconnexion", "Logout"),

            // Dashboard
            [key: "Dashboard.Title"] = ("Panoramica Esecutiva Transfrontaliera", "Grenzüberschreitende Führungsübersicht",
                                        "Aperçu exécutif transfrontalier", "Cross-Border Executive Overview"),
            [key: "Dashboard.RevenueEur"] = ("Fatturato Italia (EUR)", "Umsatz Italien (EUR)",
                                             "Chiffre d'affaires Italie (EUR)", "Revenue Italy (EUR)"),
            [key: "Dashboard.RevenueChf"] = ("Fatturato Svizzera (CHF)", "Umsatz Schweiz (CHF)",
                                             "Chiffre d'affaires Suisse (CHF)", "Revenue Switzerland (CHF)"),
            [key: "Dashboard.PendingReceivables"] = ("Crediti da Incassare", "Offene Forderungen", "Créances en suspens",
                                                     "Pending Receivables"),
            [key: "Dashboard.VatLiability"] = ("Debito IVA Stimato", "Geschätzte MWST-Schuld", "Dette TVA estimée",
                                               "Estimated VAT Liability"),
            [key: "Dashboard.NextTaxDeadline"] = ("Prossima Scadenza Fiscale", "Nächste Steuerfrist",
                                                  "Prochaine échéance fiscale", "Next Tax Deadline"),

            // Clients
            [key: "Clients.Title"] = ("Anagrafica Clienti Transfrontaliera", "Grenzüberschreitende Kundenkartei",
                                      "Fichier clients transfrontalier", "Cross-Border Client Registry"),
            [key: "Clients.NewClient"]    = ("+ Nuovo Cliente", "+ Neuer Kunde", "+ Nouveau client", "+ New Client"),
            [key: "Clients.BusinessName"] = ("Ragione Sociale", "Firmenname", "Raison sociale", "Business Name"),
            [key: "Clients.Jurisdiction"] = ("Giurisdizione", "Rechtsraum", "Juridiction", "Jurisdiction"),
            [key: "Clients.VatNumber"]    = ("Partita IVA / UID", "MWST-Nr. / UID", "N° TVA / IDE", "VAT Number / UID"),
            [key: "Clients.FiscalCode"]   = ("Codice Fiscale", "Steuernummer", "Code fiscal", "Tax / Fiscal Code"),
            [key: "Clients.SdiCode"]      = ("Codice SDI", "SDI-Code", "Code SDI", "SDI Code"),
            [key: "Clients.QrIban"]       = ("QR-IBAN Svizzero", "Schweizer QR-IBAN", "QR-IBAN Suisse", "Swiss QR-IBAN"),
            [key: "Clients.Address"]      = ("Indirizzo", "Adresse", "Adresse", "Address"),

            // Invoices
            [key: "Invoices.Title"] = ("Fatturazione Elettronica SDI & Swiss QR-Bill", "E-Rechnung SDI & Swiss QR-Bill",
                                       "Facturation électronique SDI & QR-Bill", "SDI E-Invoicing & Swiss QR-Bill"),
            [key: "Invoices.GenerateSdiXml"] = ("Genera XML FatturaPA", "FatturaPA XML generieren", "Générer XML FatturaPA",
                                                "Generate FatturaPA XML"),
            [key: "Invoices.GenerateQrBill"] = ("Crea Polizza Swiss QR", "Swiss QR-Rechnung erstellen",
                                                "Créer QR-Facture Suisse", "Create Swiss QR-Bill"),
            [key: "Invoices.StatusDelivered"] = ("Consegnato SDI", "SDI zugestellt", "Livré SDI", "SDI Delivered"),
            [key: "Invoices.StatusDraft"]     = ("Bozza", "Entwurf", "Brouillon", "Draft"),

            // Tire Hotel
            [key: "TireHotel.Title"] = ("Modulo Automotive & Hotel Pneumatici", "Automobilmodul & Reifenhotel",
                                        "Module automobile & Hôtel à pneus", "Automotive & Tire Hotel"),
            [key: "TireHotel.LicensePlate"] = ("Targa Veicolo", "Kontrollschild / Kennzeichen", "Plaque d'immatriculation",
                                               "License Plate"),
            [key: "TireHotel.TreadDepth"] = ("Battistrada (mm)", "Profiltiefe (mm)", "Profondeur sculpture (mm)",
                                             "Tread Depth (mm)"),
            [key: "TireHotel.RackLocation"] = ("Posizione Scaffale / Rack", "Regalplatz / Koordinate", "Emplacement étagère",
                                               "Rack Location"),
            [key: "TireHotel.Season"] = ("Stagione", "Saison", "Saison", "Season"),

            // Time Tracking
            [key: "Time.Title"] = ("Rilevazione Presenze (D.Lgs 66/2003 & Svizzera ArG)", "Zeiterfassung (ArG / OLL 1 & IT)",
                                   "Gestion des temps (ArG & IT)", "Time Tracking (Swiss ArG & IT)"),
            [key: "Time.ClockIn"]    = ("TIMBRA ENTRATA", "KOMMEN", "ENTRÉE", "CLOCK IN"),
            [key: "Time.ClockOut"]   = ("TIMBRA USCITA", "GEHEN", "SORTIE", "CLOCK OUT"),
            [key: "Time.TotalHours"] = ("Ore Nette Lavorate", "Nettoarbeitszeit", "Heures nettes", "Net Work Hours"),

            // Localization Admin
            [key: "LocAdmin.Title"] = ("Traduzioni Dinamiche Utente (Persistenza SQLite)",
                                       "Dynamische Übersetzungen (SQLite)", "Traductions dynamiques (SQLite)",
                                       "Dynamic User Translations (SQLite)"),
            [key: "LocAdmin.Search"] = ("Cerca chiave...", "Schlüssel suchen...", "Rechercher clé...", "Search key..."),
            [key: "LocAdmin.Save"] = ("Salva Modifiche su SQLite", "In SQLite speichern", "Enregistrer dans SQLite",
                                      "Save Changes to SQLite"),
            [key: "LocAdmin.CustomValue"] = ("Valore Personalizzato Utente", "Benutzerdefinierter Wert",
                                             "Valeur personnalisée", "User Custom Value"),
            [key: "LocAdmin.DefaultValue"] = ("Valore Predefinito di Sistema", "Standardwert", "Valeur par défaut",
                                              "System Default Value"),

            // General Common
            [key: "Common.Save"]   = ("Salva", "Speichern", "Enregistrer", "Save"),
            [key: "Common.Cancel"] = ("Annulla", "Abbrechen", "Annuler", "Cancel"),
            [key: "Common.Search"] = ("Cerca", "Suchen", "Rechercher", "Search"),
            [key: "Common.Filter"] = ("Filtra", "Filtern", "Filtrer", "Filter"),
            [key: "Common.Status"] = ("Stato", "Status", "Statut", "Status"),
            [key: "Common.Total"]  = ("Totale", "Total", "Total", "Total"),
        };

}
