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
            var tenantIt = Tenant.Create(
                                         code: "TENANT-IT",
                                         businessName: "FatturaFacile Italia S.r.l.",
                                         countryCode: "IT",
                                         baseCurrency: Currency.EUR,
                                         contactEmail: "amministrazione@fatturafacile.it",
                                         vatNumber: "IT01234567890",
                                         fiscalCode: "01234567890",
                                         sdiCode: "M5UXCR1");

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

            var tenantCh = Tenant.Create(
                                         code: "TENANT-CH",
                                         businessName: "FatturaFacile Svizzera SA",
                                         countryCode: "CH",
                                         baseCurrency: Currency.CHF,
                                         contactEmail: "info@fatturafacile.ch",
                                         swissUid: "CHE-123.456.789 MWST",
                                         qrIban: "CH4431999123000889012");

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
            var clientIt = Client.CreateItalian(
                                                tenantId: tenantIt.Id,
                                                businessName: "Rossi Automazioni Industriali S.p.A.",
                                                fiscalCode: new FiscalCode(value: "RSSMRA80A01F205X"),
                                                vatNumber: new VatNumber(
                                                                         value: "11223344556",
                                                                         countryCode: "IT"),
                                                sdiCode: "SUBM170",
                                                pecEmail: "rossi.automazioni@pec.it",
                                                address: new Address(
                                                                     street: "Via delle Industrie 45",
                                                                     city: "Monza",
                                                                     postalCode: "20900",
                                                                     provinceOrCanton: "MB",
                                                                     countryCode: "IT"),
                                                email: "ordini@rossi-auto.it",
                                                phone: "+39 039 123456");

            // Seed Cross-Border Client for Tenant CH
            var clientCh = Client.CreateSwiss(
                                              tenantId: tenantCh.Id,
                                              businessName: "Gotthard Engineering AG",
                                              swissUid: "CHE-987.654.321",
                                              cantonalCode: "TI",
                                              qrIban: new QrIban(value: "CH4431999123000889012"),
                                              address: new Address(
                                                                   street: "Via San Gottardo 102",
                                                                   city: "Bellinzona",
                                                                   postalCode: "6500",
                                                                   provinceOrCanton: "TI",
                                                                   countryCode: "CH"),
                                              email: "billing@gotthard-eng.ch",
                                              phone: "+41 91 820 00 00");

            context.Clients.AddRange(
                                     entities: [clientIt, clientCh]);

            // 5. Seed Automotive Vehicle & Tires
            var vehicle = CustomerVehicle.Create(
                                                 tenantId: tenantIt.Id,
                                                 clientId: clientIt.Id,
                                                 licensePlate: "GE945KZ",
                                                 vin: "WBA3A5C50DF123456",
                                                 brand: "BMW",
                                                 model: "Serie 3 Touring",
                                                 year: 2023,
                                                 jurisdiction: Jurisdiction.Italy,
                                                 km: 38500);

            _ = context.CustomerVehicles.Add(entity: vehicle);

            var tireRecord = TireHotelRecord.CheckIn(
                                                     tenantId: tenantIt.Id,
                                                     vehicleId: vehicle.Id,
                                                     season: TireSeason.Winter,
                                                     brand: "Michelin Pilot Alpin 5",
                                                     dimension: "225/45 R17 94V",
                                                     dotCode: "3824",
                                                     flMm: 7.2m,
                                                     frMm: 7.1m,
                                                     rlMm: 6.8m,
                                                     rrMm: 6.9m,
                                                     rackCoordinate: "RACK-B-12-3");

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

            var sampleQrBill = SwissQrBill.Create(
                                                  tenantId: tenantCh.Id,
                                                  billNumber: "QR-2026/088",
                                                  billDate: DateTime.UtcNow.AddDays(value: -1),
                                                  clientId: clientCh.Id,
                                                  creditorName: "FatturaFacile Svizzera SA",
                                                  creditorAddress: new Address(
                                                                               street: "Via Nassa 12",
                                                                               city: "Lugano",
                                                                               postalCode: "6900",
                                                                               provinceOrCanton: "TI",
                                                                               countryCode: "CH"),
                                                  creditorQrIban: new QrIban(value: "CH4431999123000889012"),
                                                  debtorName: "Gotthard Engineering AG",
                                                  debtorAddress: new Address(
                                                                             street: "Via San Gottardo 102",
                                                                             city: "Bellinzona",
                                                                             postalCode: "6500",
                                                                             provinceOrCanton: "TI",
                                                                             countryCode: "CH"),
                                                  rawAmount: Money.Chf(amount: 4850.25m),
                                                  referenceNumber: "210000000003139471196009917",
                                                  dueDate: DateTime.UtcNow.AddDays(value: 30),
                                                  message: "Incarico perizia tecnica fiduciaria cantone Ticino");

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
                                                                           defaultValue: en)
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
