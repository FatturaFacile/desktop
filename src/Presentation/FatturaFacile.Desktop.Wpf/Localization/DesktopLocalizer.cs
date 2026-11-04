using System.ComponentModel;
using System.Globalization;

namespace FatturaFacile.Desktop.Wpf.Localization;

/// <summary>
/// Service providing localized strings for the desktop interface across English, Italian, German, and French.
/// </summary>
public interface IDesktopLocalizer : INotifyPropertyChanged
{
    string CurrentCulture { get; }
    void SetCulture(string culture);
    string this[string key] { get; }
    string GetString(string key);
}

/// <summary>
/// Reactive localization service supporting runtime 4-language switching without application restart.
/// </summary>
public sealed class DesktopLocalizer : IDesktopLocalizer
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentCulture = "en";
    public string CurrentCulture => _currentCulture;

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        // English (Default)
        ["en"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["App.Title"] = "FatturaFacile ERP - Enterprise Desktop",
            ["Nav.Dashboard"] = "Dashboard / Home",
            ["Nav.SalesInvoicing"] = "Sales & Invoicing",
            ["Nav.Invoices"] = "Invoices",
            ["Nav.Proformas"] = "Proformas",
            ["Nav.CreditNotes"] = "Credit Notes",
            ["Nav.SwissQrBill"] = "Swiss QR-Bill",
            ["Nav.ItalianSdi"] = "Italian SDI Export",
            ["Nav.PurchasesExpenses"] = "Purchases & Expenses",
            ["Nav.SupplierInvoices"] = "Supplier Invoices",
            ["Nav.Expenses"] = "Expenses",
            ["Nav.VendorOrders"] = "Vendor Orders",
            ["Nav.AccountingFinance"] = "Accounting & Finance",
            ["Nav.GeneralLedger"] = "General Ledger",
            ["Nav.ChartOfAccounts"] = "Chart of Accounts",
            ["Nav.VatReconciliation"] = "VAT Reconciliation",
            ["Nav.BalanceSheet"] = "Balance Sheet",
            ["Nav.CrmContacts"] = "CRM & Contacts",
            ["Nav.Customers"] = "Customers",
            ["Nav.Vendors"] = "Vendors",
            ["Nav.ContactPersons"] = "Contact Persons",
            ["Nav.Leads"] = "Leads",
            ["Nav.WarehouseProducts"] = "Warehouse & Products",
            ["Nav.ProductCatalog"] = "Product Catalog",
            ["Nav.StockLevels"] = "Stock Levels",
            ["Nav.PriceLists"] = "Price Lists",
            ["Nav.AdministrationSecurity"] = "Administration & Security",
            ["Nav.UserManagement"] = "User Management",
            ["Nav.RolePermissions"] = "Role Permissions",
            ["Nav.TenantSettings"] = "Tenant Settings",
            ["Nav.AuditLog"] = "Audit Log",
            ["Nav.BackupRestore"] = "Backup / Restore",

            ["Login.Title"] = "Sign in to FatturaFacile ERP",
            ["Login.Subtitle"] = "Offline-First Enterprise Resource Planning Client",
            ["Login.Tenant"] = "Company Code / Tenant Slug",
            ["Login.Email"] = "Email Address",
            ["Login.Password"] = "Password",
            ["Login.SignInButton"] = "Sign In",
            ["Login.SigningIn"] = "Authenticating with Argon2id...",
            ["Login.Error"] = "Authentication failed. Please verify your credentials and tenant slug.",

            ["Header.GlobalSearch"] = "Search invoices, customers, accounts...",
            ["Header.NewInvoice"] = "New Invoice",
            ["Header.NewCustomer"] = "New Customer",
            ["Header.SyncStatus"] = "Local SQLite Offline Mode: Connected",
            ["Header.SignOut"] = "Sign Out",

            ["Status.Ready"] = "System Ready",
            ["Status.OfflineReady"] = "Offline Engine Active (SQLite)",
            ["Status.Version"] = "Version 1.0.0 (MSIX Store)",
            ["Status.BackgroundTasks"] = "Background Tasks: 0 Active",

            ["Invoices.Title"] = "Invoices",
            ["Invoices.New"] = "Create Invoice",
            ["Invoices.Refresh"] = "Refresh",
            ["Invoices.ExportExcel"] = "Export Excel",
            ["Invoices.ExportPdf"] = "Export PDF",
            ["Invoices.InvoiceNumber"] = "Invoice #",
            ["Invoices.Date"] = "Date",
            ["Invoices.DueDate"] = "Due Date",
            ["Invoices.Customer"] = "Customer ID",
            ["Invoices.TotalNet"] = "Net Amount",
            ["Invoices.TotalTax"] = "VAT",
            ["Invoices.Rounding"] = "Rounding (CHF)",
            ["Invoices.TotalGross"] = "Gross Total",
            ["Invoices.Currency"] = "Currency",
            ["Invoices.Status"] = "Status",
            ["Invoices.Actions"] = "Actions",
            ["Invoices.ViewDetail"] = "View / Edit",

            ["InvoiceDetail.Title"] = "Invoice Details",
            ["InvoiceDetail.Header"] = "Header Information",
            ["InvoiceDetail.Items"] = "Line Items",
            ["InvoiceDetail.AddItem"] = "Add Item",
            ["InvoiceDetail.RemoveItem"] = "Remove Item",
            ["InvoiceDetail.Description"] = "Description",
            ["InvoiceDetail.Quantity"] = "Qty",
            ["InvoiceDetail.UnitPrice"] = "Unit Price",
            ["InvoiceDetail.TaxRate"] = "VAT %",
            ["InvoiceDetail.Discount"] = "Discount %",
            ["InvoiceDetail.LineTotal"] = "Line Total",
            ["InvoiceDetail.Save"] = "Save Invoice",
            ["InvoiceDetail.GenerateQrBill"] = "Swiss QR-Bill Preview",
            ["InvoiceDetail.Cancel"] = "Back to List",

            ["Dashboard.TotalSales"] = "Total Revenue",
            ["Dashboard.OpenInvoices"] = "Pending Invoices",
            ["Dashboard.OverdueInvoices"] = "Overdue Invoices",
            ["Dashboard.ActiveCustomers"] = "Active Customers",
            ["Dashboard.RecentActivity"] = "Recent Invoices & Transactions"
        },

        // Italian
        ["it"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["App.Title"] = "FatturaFacile ERP - Desktop Aziendale",
            ["Nav.Dashboard"] = "Dashboard / Home",
            ["Nav.SalesInvoicing"] = "Vendite e Fatturazione",
            ["Nav.Invoices"] = "Fatture",
            ["Nav.Proformas"] = "Fatture Proforma",
            ["Nav.CreditNotes"] = "Note di Credito",
            ["Nav.SwissQrBill"] = "Polizza Svizzera QR",
            ["Nav.ItalianSdi"] = "Esportazione SDI Italia",
            ["Nav.PurchasesExpenses"] = "Acquisti e Spese",
            ["Nav.SupplierInvoices"] = "Fatture Fornitori",
            ["Nav.Expenses"] = "Spese e Rimborsi",
            ["Nav.VendorOrders"] = "Ordini a Fornitori",
            ["Nav.AccountingFinance"] = "Contabilità e Finanza",
            ["Nav.GeneralLedger"] = "Libro Giornale",
            ["Nav.ChartOfAccounts"] = "Piano dei Conti",
            ["Nav.VatReconciliation"] = "Liquidazione IVA",
            ["Nav.BalanceSheet"] = "Bilancio d'Esercizio",
            ["Nav.CrmContacts"] = "CRM e Contatti",
            ["Nav.Customers"] = "Clienti",
            ["Nav.Vendors"] = "Fornitori",
            ["Nav.ContactPersons"] = "Referenti",
            ["Nav.Leads"] = "Opportunità",
            ["Nav.WarehouseProducts"] = "Magazzino e Prodotti",
            ["Nav.ProductCatalog"] = "Catalogo Prodotti",
            ["Nav.StockLevels"] = "Giacenze Magazzino",
            ["Nav.PriceLists"] = "Listini Prezzi",
            ["Nav.AdministrationSecurity"] = "Amministrazione e Sicurezza",
            ["Nav.UserManagement"] = "Gestione Utenti",
            ["Nav.RolePermissions"] = "Ruoli e Permessi",
            ["Nav.TenantSettings"] = "Configurazione Azienda",
            ["Nav.AuditLog"] = "Registro di Controllo",
            ["Nav.BackupRestore"] = "Backup e Ripristino",

            ["Login.Title"] = "Accedi a FatturaFacile ERP",
            ["Login.Subtitle"] = "Client Gestionale Enterprise Offline-First",
            ["Login.Tenant"] = "Codice Azienda / Slug",
            ["Login.Email"] = "Indirizzo Email",
            ["Login.Password"] = "Password",
            ["Login.SignInButton"] = "Accedi",
            ["Login.SigningIn"] = "Autenticazione Argon2id in corso...",
            ["Login.Error"] = "Accesso fallito. Verificare le credenziali e il codice azienda.",

            ["Header.GlobalSearch"] = "Cerca fatture, clienti, conti...",
            ["Header.NewInvoice"] = "Nuova Fattura",
            ["Header.NewCustomer"] = "Nuovo Cliente",
            ["Header.SyncStatus"] = "Modalità Offline SQLite: Connesso",
            ["Header.SignOut"] = "Disconnetti",

            ["Status.Ready"] = "Sistema Pronto",
            ["Status.OfflineReady"] = "Motore Offline Attivo (SQLite)",
            ["Status.Version"] = "Versione 1.0.0 (MSIX Store)",
            ["Status.BackgroundTasks"] = "Attività in background: 0 Attive",

            ["Invoices.Title"] = "Fatture di Vendita",
            ["Invoices.New"] = "Crea Fattura",
            ["Invoices.Refresh"] = "Aggiorna",
            ["Invoices.ExportExcel"] = "Esporta Excel",
            ["Invoices.ExportPdf"] = "Esporta PDF",
            ["Invoices.InvoiceNumber"] = "N. Fattura",
            ["Invoices.Date"] = "Data",
            ["Invoices.DueDate"] = "Scadenza",
            ["Invoices.Customer"] = "ID Cliente",
            ["Invoices.TotalNet"] = "Imponibile",
            ["Invoices.TotalTax"] = "IVA",
            ["Invoices.Rounding"] = "Arrotondamento (CHF)",
            ["Invoices.TotalGross"] = "Totale Fattura",
            ["Invoices.Currency"] = "Valuta",
            ["Invoices.Status"] = "Stato",
            ["Invoices.Actions"] = "Azioni",
            ["Invoices.ViewDetail"] = "Visualizza / Modifica",

            ["InvoiceDetail.Title"] = "Dettaglio Fattura",
            ["InvoiceDetail.Header"] = "Dati Principali",
            ["InvoiceDetail.Items"] = "Righe Articolo",
            ["InvoiceDetail.AddItem"] = "Aggiungi Riga",
            ["InvoiceDetail.RemoveItem"] = "Rimuovi Riga",
            ["InvoiceDetail.Description"] = "Descrizione",
            ["InvoiceDetail.Quantity"] = "Quantità",
            ["InvoiceDetail.UnitPrice"] = "Prezzo Unitario",
            ["InvoiceDetail.TaxRate"] = "Aliquota IVA %",
            ["InvoiceDetail.Discount"] = "Sconto %",
            ["InvoiceDetail.LineTotal"] = "Totale Riga",
            ["InvoiceDetail.Save"] = "Salva Fattura",
            ["InvoiceDetail.GenerateQrBill"] = "Anteprima Polizza QR Svizzera",
            ["InvoiceDetail.Cancel"] = "Torna all'elenco",

            ["Dashboard.TotalSales"] = "Fatturato Totale",
            ["Dashboard.OpenInvoices"] = "Fatture da Incassare",
            ["Dashboard.OverdueInvoices"] = "Fatture Scadute",
            ["Dashboard.ActiveCustomers"] = "Clienti Attivi",
            ["Dashboard.RecentActivity"] = "Ultime Fatture e Movimenti"
        },

        // German
        ["de"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["App.Title"] = "FatturaFacile ERP - Desktop Enterprise",
            ["Nav.Dashboard"] = "Dashboard / Startseite",
            ["Nav.SalesInvoicing"] = "Verkauf & Rechnungsstellung",
            ["Nav.Invoices"] = "Rechnungen",
            ["Nav.Proformas"] = "Proforma-Rechnungen",
            ["Nav.CreditNotes"] = "Gutschriften",
            ["Nav.SwissQrBill"] = "Schweizer QR-Rechnung",
            ["Nav.ItalianSdi"] = "Italien SDI-Export",
            ["Nav.PurchasesExpenses"] = "Einkauf & Ausgaben",
            ["Nav.SupplierInvoices"] = "Lieferantenrechnungen",
            ["Nav.Expenses"] = "Spesen",
            ["Nav.VendorOrders"] = "Lieferantenbestellungen",
            ["Nav.AccountingFinance"] = "Buchhaltung & Finanzen",
            ["Nav.GeneralLedger"] = "Hauptbuch",
            ["Nav.ChartOfAccounts"] = "Kontenplan",
            ["Nav.VatReconciliation"] = "MWST-Abrechnung",
            ["Nav.BalanceSheet"] = "Bilanz",
            ["Nav.CrmContacts"] = "CRM & Kontakte",
            ["Nav.Customers"] = "Kunden",
            ["Nav.Vendors"] = "Lieferanten",
            ["Nav.ContactPersons"] = "Ansprechpartner",
            ["Nav.Leads"] = "Interessenten",
            ["Nav.WarehouseProducts"] = "Lager & Produkte",
            ["Nav.ProductCatalog"] = "Produktkatalog",
            ["Nav.StockLevels"] = "Lagerbestände",
            ["Nav.PriceLists"] = "Preislisten",
            ["Nav.AdministrationSecurity"] = "Administration & Sicherheit",
            ["Nav.UserManagement"] = "Benutzerverwaltung",
            ["Nav.RolePermissions"] = "Rollen & Berechtigungen",
            ["Nav.TenantSettings"] = "Mandanteneinstellungen",
            ["Nav.AuditLog"] = "Audit-Protokoll",
            ["Nav.BackupRestore"] = "Datensicherung & Wiederherstellung",

            ["Login.Title"] = "Anmeldung bei FatturaFacile ERP",
            ["Login.Subtitle"] = "Offline-fähiger Enterprise ERP Client",
            ["Login.Tenant"] = "Mandanten-Code / Slug",
            ["Login.Email"] = "E-Mail-Adresse",
            ["Login.Password"] = "Passwort",
            ["Login.SignInButton"] = "Anmelden",
            ["Login.SigningIn"] = "Argon2id-Authentifizierung läuft...",
            ["Login.Error"] = "Anmeldung fehlgeschlagen. Bitte Zugangsdaten und Mandantencode prüfen.",

            ["Header.GlobalSearch"] = "Rechnungen, Kunden, Konten durchsuchen...",
            ["Header.NewInvoice"] = "Neue Rechnung",
            ["Header.NewCustomer"] = "Neuer Kunde",
            ["Header.SyncStatus"] = "Lokaler SQLite Offline-Modus: Verbunden",
            ["Header.SignOut"] = "Abmelden",

            ["Status.Ready"] = "Bereit",
            ["Status.OfflineReady"] = "Offline-Modul Aktiv (SQLite)",
            ["Status.Version"] = "Version 1.0.0 (MSIX Store)",
            ["Status.BackgroundTasks"] = "Hintergrundaufgaben: 0 Aktiv",

            ["Invoices.Title"] = "Ausgangsrechnungen",
            ["Invoices.New"] = "Rechnung Erstellen",
            ["Invoices.Refresh"] = "Aktualisieren",
            ["Invoices.ExportExcel"] = "Excel-Export",
            ["Invoices.ExportPdf"] = "PDF-Export",
            ["Invoices.InvoiceNumber"] = "Rechnungs-Nr.",
            ["Invoices.Date"] = "Datum",
            ["Invoices.DueDate"] = "Fälligkeitsdatum",
            ["Invoices.Customer"] = "Kunden-ID",
            ["Invoices.TotalNet"] = "Nettobetrag",
            ["Invoices.TotalTax"] = "MWST",
            ["Invoices.Rounding"] = "Rundung (CHF)",
            ["Invoices.TotalGross"] = "Bruttobetrag",
            ["Invoices.Currency"] = "Währung",
            ["Invoices.Status"] = "Status",
            ["Invoices.Actions"] = "Aktionen",
            ["Invoices.ViewDetail"] = "Anzeigen / Bearbeiten",

            ["InvoiceDetail.Title"] = "Rechnungsdetails",
            ["InvoiceDetail.Header"] = "Kopfdaten",
            ["InvoiceDetail.Items"] = "Positionen",
            ["InvoiceDetail.AddItem"] = "Position hinzufügen",
            ["InvoiceDetail.RemoveItem"] = "Position entfernen",
            ["InvoiceDetail.Description"] = "Beschreibung",
            ["InvoiceDetail.Quantity"] = "Menge",
            ["InvoiceDetail.UnitPrice"] = "Einzelpreis",
            ["InvoiceDetail.TaxRate"] = "MWST %",
            ["InvoiceDetail.Discount"] = "Rabatt %",
            ["InvoiceDetail.LineTotal"] = "Gesamtpreis",
            ["InvoiceDetail.Save"] = "Rechnung Speichern",
            ["InvoiceDetail.GenerateQrBill"] = "Schweizer QR-Rechnung Vorschau",
            ["InvoiceDetail.Cancel"] = "Zurück zur Übersicht",

            ["Dashboard.TotalSales"] = "Gesamtumsatz",
            ["Dashboard.OpenInvoices"] = "Offene Rechnungen",
            ["Dashboard.OverdueInvoices"] = "Überfällige Rechnungen",
            ["Dashboard.ActiveCustomers"] = "Aktive Kunden",
            ["Dashboard.RecentActivity"] = "Letzte Rechnungen & Buchungen"
        },

        // French
        ["fr"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["App.Title"] = "FatturaFacile ERP - Desktop Entreprise",
            ["Nav.Dashboard"] = "Tableau de Bord / Accueil",
            ["Nav.SalesInvoicing"] = "Ventes & Facturation",
            ["Nav.Invoices"] = "Factures",
            ["Nav.Proformas"] = "Factures Proforma",
            ["Nav.CreditNotes"] = "Notes de Crédit",
            ["Nav.SwissQrBill"] = "QR-Facture Suisse",
            ["Nav.ItalianSdi"] = "Exportation SDI Italie",
            ["Nav.PurchasesExpenses"] = "Achats & Dépenses",
            ["Nav.SupplierInvoices"] = "Factures Fournisseurs",
            ["Nav.Expenses"] = "Notes de Frais",
            ["Nav.VendorOrders"] = "Commandes Fournisseurs",
            ["Nav.AccountingFinance"] = "Comptabilité & Finance",
            ["Nav.GeneralLedger"] = "Grand Livre",
            ["Nav.ChartOfAccounts"] = "Plan Comptable",
            ["Nav.VatReconciliation"] = "Déclaration TVA",
            ["Nav.BalanceSheet"] = "Bilan Comptable",
            ["Nav.CrmContacts"] = "CRM & Contacts",
            ["Nav.Customers"] = "Clients",
            ["Nav.Vendors"] = "Fournisseurs",
            ["Nav.ContactPersons"] = "Contacts",
            ["Nav.Leads"] = "Prospects",
            ["Nav.WarehouseProducts"] = "Entrepôt & Articles",
            ["Nav.ProductCatalog"] = "Catalogue Produits",
            ["Nav.StockLevels"] = "Niveaux de Stock",
            ["Nav.PriceLists"] = "Listes de Prix",
            ["Nav.AdministrationSecurity"] = "Administration & Sécurité",
            ["Nav.UserManagement"] = "Gestion des Utilisateurs",
            ["Nav.RolePermissions"] = "Rôles & Permissions",
            ["Nav.TenantSettings"] = "Paramètres Entreprise",
            ["Nav.AuditLog"] = "Journal d'Audit",
            ["Nav.BackupRestore"] = "Sauvegarde & Restauration",

            ["Login.Title"] = "Connexion à FatturaFacile ERP",
            ["Login.Subtitle"] = "Client ERP Entreprise Offline-First",
            ["Login.Tenant"] = "Code Entreprise / Slug",
            ["Login.Email"] = "Adresse Email",
            ["Login.Password"] = "Mot de Passe",
            ["Login.SignInButton"] = "Se Connecter",
            ["Login.SigningIn"] = "Authentification Argon2id en cours...",
            ["Login.Error"] = "Échec de connexion. Veuillez vérifier vos identifiants et le code entreprise.",

            ["Header.GlobalSearch"] = "Rechercher factures, clients, comptes...",
            ["Header.NewInvoice"] = "Nouvelle Facture",
            ["Header.NewCustomer"] = "Nouveau Client",
            ["Header.SyncStatus"] = "Mode Offline SQLite : Connecté",
            ["Header.SignOut"] = "Se Déconnecter",

            ["Status.Ready"] = "Système Prêt",
            ["Status.OfflineReady"] = "Moteur Hors Ligne Actif (SQLite)",
            ["Status.Version"] = "Version 1.0.0 (MSIX Store)",
            ["Status.BackgroundTasks"] = "Tâches d'arrière-plan : 0 Active",

            ["Invoices.Title"] = "Factures Clients",
            ["Invoices.New"] = "Créer une Facture",
            ["Invoices.Refresh"] = "Actualiser",
            ["Invoices.ExportExcel"] = "Export Excel",
            ["Invoices.ExportPdf"] = "Export PDF",
            ["Invoices.InvoiceNumber"] = "N° de Facture",
            ["Invoices.Date"] = "Date",
            ["Invoices.DueDate"] = "Échéance",
            ["Invoices.Customer"] = "ID Client",
            ["Invoices.TotalNet"] = "Total Hors Taxe",
            ["Invoices.TotalTax"] = "TVA",
            ["Invoices.Rounding"] = "Arrondi (CHF)",
            ["Invoices.TotalGross"] = "Total TTC",
            ["Invoices.Currency"] = "Devise",
            ["Invoices.Status"] = "Statut",
            ["Invoices.Actions"] = "Actions",
            ["Invoices.ViewDetail"] = "Voir / Modifier",

            ["InvoiceDetail.Title"] = "Détail de la Facture",
            ["InvoiceDetail.Header"] = "Informations Générales",
            ["InvoiceDetail.Items"] = "Lignes d'Articles",
            ["InvoiceDetail.AddItem"] = "Ajouter une Ligne",
            ["InvoiceDetail.RemoveItem"] = "Supprimer la Ligne",
            ["InvoiceDetail.Description"] = "Description",
            ["InvoiceDetail.Quantity"] = "Quantité",
            ["InvoiceDetail.UnitPrice"] = "Prix Unitaire",
            ["InvoiceDetail.TaxRate"] = "Taux TVA %",
            ["InvoiceDetail.Discount"] = "Remise %",
            ["InvoiceDetail.LineTotal"] = "Total Ligne",
            ["InvoiceDetail.Save"] = "Enregistrer la Facture",
            ["InvoiceDetail.GenerateQrBill"] = "Aperçu QR-Facture Suisse",
            ["InvoiceDetail.Cancel"] = "Retour à la liste",

            ["Dashboard.TotalSales"] = "Chiffre d'Affaires Total",
            ["Dashboard.OpenInvoices"] = "Factures en Attente",
            ["Dashboard.OverdueInvoices"] = "Factures Échues",
            ["Dashboard.ActiveCustomers"] = "Clients Actifs",
            ["Dashboard.RecentActivity"] = "Dernières Factures & Écritures"
        }
    };

    public string this[string key] => GetString(key);

    public string GetString(string key)
    {
        var culture = _currentCulture.Length >= 2 ? _currentCulture[..2].ToLowerInvariant() : "en";
        if (Translations.TryGetValue(culture, out var langDict) && langDict.TryGetValue(key, out var val))
        {
            return val;
        }

        // Fallback to English
        if (Translations["en"].TryGetValue(key, out var fallbackVal))
        {
            return fallbackVal;
        }

        return key;
    }

    public void SetCulture(string culture)
    {
        var normalized = string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim();
        if (_currentCulture != normalized)
        {
            _currentCulture = normalized;
            try
            {
                var cultureInfo = new CultureInfo(normalized.Length >= 2 ? normalized[..2] : "en");
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;
            }
            catch
            {
                // Ignore unsupported culture info initialization
            }

            // Notify all indexer and property bindings of language switch
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        }
    }
}
