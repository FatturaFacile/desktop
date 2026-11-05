// <copyright file="Strings.Designer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#nullable enable


namespace FatturaFacile.Wpf.Localization;

using System.Globalization;
using System.Resources;

/// <summary>
/// A strongly-typed resource class, for looking up localized strings.
/// </summary>
public sealed class Strings
{
    private static ResourceManager? resourceMan;
    private static CultureInfo? resourceCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="Strings"/> class.
    /// </summary>
    public Strings()
    {
    }

    /// <summary>
    /// Gets the cached ResourceManager instance used by this class.
    /// </summary>
    public static ResourceManager ResourceManager
    {
        get
        {
            if (resourceMan == null)
            {
                resourceMan = new ResourceManager("FatturaFacile.Wpf.Localization.Strings", typeof(Strings).Assembly);
            }

            return resourceMan;
        }
    }

    /// <summary>
    /// Gets or sets the culture override for lookups.
    /// </summary>
    public static CultureInfo? Culture
    {
        get => resourceCulture;
        set => resourceCulture = value;
    }

    /// <summary>
    /// Looks up a localized string similar to key 'App_Title'.
    /// </summary>
    public static string App_Title => ResourceManager.GetString("App_Title", resourceCulture) ?? "App_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'App_BrandName'.
    /// </summary>
    public static string App_BrandName => ResourceManager.GetString("App_BrandName", resourceCulture) ?? "App_BrandName";

    /// <summary>
    /// Looks up a localized string similar to key 'App_Tagline'.
    /// </summary>
    public static string App_Tagline => ResourceManager.GetString("App_Tagline", resourceCulture) ?? "App_Tagline";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_SectionTitle'.
    /// </summary>
    public static string Nav_SectionTitle => ResourceManager.GetString("Nav_SectionTitle", resourceCulture) ?? "Nav_SectionTitle";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_Invoicing'.
    /// </summary>
    public static string Nav_Invoicing => ResourceManager.GetString("Nav_Invoicing", resourceCulture) ?? "Nav_Invoicing";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_Accounting'.
    /// </summary>
    public static string Nav_Accounting => ResourceManager.GetString("Nav_Accounting", resourceCulture) ?? "Nav_Accounting";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_Contacts'.
    /// </summary>
    public static string Nav_Contacts => ResourceManager.GetString("Nav_Contacts", resourceCulture) ?? "Nav_Contacts";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_Products'.
    /// </summary>
    public static string Nav_Products => ResourceManager.GetString("Nav_Products", resourceCulture) ?? "Nav_Products";

    /// <summary>
    /// Looks up a localized string similar to key 'Nav_Payroll'.
    /// </summary>
    public static string Nav_Payroll => ResourceManager.GetString("Nav_Payroll", resourceCulture) ?? "Nav_Payroll";

    /// <summary>
    /// Looks up a localized string similar to key 'Footer_ActiveModule'.
    /// </summary>
    public static string Footer_ActiveModule => ResourceManager.GetString("Footer_ActiveModule", resourceCulture) ?? "Footer_ActiveModule";

    /// <summary>
    /// Looks up a localized string similar to key 'Footer_FiscalCountry'.
    /// </summary>
    public static string Footer_FiscalCountry => ResourceManager.GetString("Footer_FiscalCountry", resourceCulture) ?? "Footer_FiscalCountry";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_HardwareSecurity'.
    /// </summary>
    public static string Shell_HardwareSecurity => ResourceManager.GetString("Shell_HardwareSecurity", resourceCulture) ?? "Shell_HardwareSecurity";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_TenantSelector'.
    /// </summary>
    public static string Shell_TenantSelector => ResourceManager.GetString("Shell_TenantSelector", resourceCulture) ?? "Shell_TenantSelector";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_Language'.
    /// </summary>
    public static string Shell_Language => ResourceManager.GetString("Shell_Language", resourceCulture) ?? "Shell_Language";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_Lock'.
    /// </summary>
    public static string Shell_Lock => ResourceManager.GetString("Shell_Lock", resourceCulture) ?? "Shell_Lock";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_Logout'.
    /// </summary>
    public static string Shell_Logout => ResourceManager.GetString("Shell_Logout", resourceCulture) ?? "Shell_Logout";

    /// <summary>
    /// Looks up a localized string similar to key 'Shell_Currency'.
    /// </summary>
    public static string Shell_Currency => ResourceManager.GetString("Shell_Currency", resourceCulture) ?? "Shell_Currency";

    /// <summary>
    /// Looks up a localized string similar to key 'Lock_Title'.
    /// </summary>
    public static string Lock_Title => ResourceManager.GetString("Lock_Title", resourceCulture) ?? "Lock_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Lock_Subtitle'.
    /// </summary>
    public static string Lock_Subtitle => ResourceManager.GetString("Lock_Subtitle", resourceCulture) ?? "Lock_Subtitle";

    /// <summary>
    /// Looks up a localized string similar to key 'Lock_Prompt'.
    /// </summary>
    public static string Lock_Prompt => ResourceManager.GetString("Lock_Prompt", resourceCulture) ?? "Lock_Prompt";

    /// <summary>
    /// Looks up a localized string similar to key 'Lock_BiometricReason'.
    /// </summary>
    public static string Lock_BiometricReason => ResourceManager.GetString("Lock_BiometricReason", resourceCulture) ?? "Lock_BiometricReason";

    /// <summary>
    /// Looks up a localized string similar to key 'Lock_Failed'.
    /// </summary>
    public static string Lock_Failed => ResourceManager.GetString("Lock_Failed", resourceCulture) ?? "Lock_Failed";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_Title'.
    /// </summary>
    public static string Login_Title => ResourceManager.GetString("Login_Title", resourceCulture) ?? "Login_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_Subtitle'.
    /// </summary>
    public static string Login_Subtitle => ResourceManager.GetString("Login_Subtitle", resourceCulture) ?? "Login_Subtitle";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_TenantLabel'.
    /// </summary>
    public static string Login_TenantLabel => ResourceManager.GetString("Login_TenantLabel", resourceCulture) ?? "Login_TenantLabel";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_Button'.
    /// </summary>
    public static string Login_Button => ResourceManager.GetString("Login_Button", resourceCulture) ?? "Login_Button";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_BiometricReason'.
    /// </summary>
    public static string Login_BiometricReason => ResourceManager.GetString("Login_BiometricReason", resourceCulture) ?? "Login_BiometricReason";

    /// <summary>
    /// Looks up a localized string similar to key 'Login_Failed'.
    /// </summary>
    public static string Login_Failed => ResourceManager.GetString("Login_Failed", resourceCulture) ?? "Login_Failed";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Ready'.
    /// </summary>
    public static string Common_Ready => ResourceManager.GetString("Common_Ready", resourceCulture) ?? "Common_Ready";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Loading'.
    /// </summary>
    public static string Common_Loading => ResourceManager.GetString("Common_Loading", resourceCulture) ?? "Common_Loading";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Save'.
    /// </summary>
    public static string Common_Save => ResourceManager.GetString("Common_Save", resourceCulture) ?? "Common_Save";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Delete'.
    /// </summary>
    public static string Common_Delete => ResourceManager.GetString("Common_Delete", resourceCulture) ?? "Common_Delete";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Cancel'.
    /// </summary>
    public static string Common_Cancel => ResourceManager.GetString("Common_Cancel", resourceCulture) ?? "Common_Cancel";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Refresh'.
    /// </summary>
    public static string Common_Refresh => ResourceManager.GetString("Common_Refresh", resourceCulture) ?? "Common_Refresh";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Details'.
    /// </summary>
    public static string Common_Details => ResourceManager.GetString("Common_Details", resourceCulture) ?? "Common_Details";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Search'.
    /// </summary>
    public static string Common_Search => ResourceManager.GetString("Common_Search", resourceCulture) ?? "Common_Search";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Status'.
    /// </summary>
    public static string Common_Status => ResourceManager.GetString("Common_Status", resourceCulture) ?? "Common_Status";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Total'.
    /// </summary>
    public static string Common_Total => ResourceManager.GetString("Common_Total", resourceCulture) ?? "Common_Total";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Date'.
    /// </summary>
    public static string Common_Date => ResourceManager.GetString("Common_Date", resourceCulture) ?? "Common_Date";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Actions'.
    /// </summary>
    public static string Common_Actions => ResourceManager.GetString("Common_Actions", resourceCulture) ?? "Common_Actions";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_ExportPdf'.
    /// </summary>
    public static string Common_ExportPdf => ResourceManager.GetString("Common_ExportPdf", resourceCulture) ?? "Common_ExportPdf";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_ExportXml'.
    /// </summary>
    public static string Common_ExportXml => ResourceManager.GetString("Common_ExportXml", resourceCulture) ?? "Common_ExportXml";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_New'.
    /// </summary>
    public static string Common_New => ResourceManager.GetString("Common_New", resourceCulture) ?? "Common_New";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Select'.
    /// </summary>
    public static string Common_Select => ResourceManager.GetString("Common_Select", resourceCulture) ?? "Common_Select";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Error'.
    /// </summary>
    public static string Common_Error => ResourceManager.GetString("Common_Error", resourceCulture) ?? "Common_Error";

    /// <summary>
    /// Looks up a localized string similar to key 'Common_Success'.
    /// </summary>
    public static string Common_Success => ResourceManager.GetString("Common_Success", resourceCulture) ?? "Common_Success";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Title'.
    /// </summary>
    public static string Invoicing_Title => ResourceManager.GetString("Invoicing_Title", resourceCulture) ?? "Invoicing_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_ListHeader'.
    /// </summary>
    public static string Invoicing_ListHeader => ResourceManager.GetString("Invoicing_ListHeader", resourceCulture) ?? "Invoicing_ListHeader";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Number'.
    /// </summary>
    public static string Invoicing_Number => ResourceManager.GetString("Invoicing_Number", resourceCulture) ?? "Invoicing_Number";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Customer'.
    /// </summary>
    public static string Invoicing_Customer => ResourceManager.GetString("Invoicing_Customer", resourceCulture) ?? "Invoicing_Customer";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_IssueDate'.
    /// </summary>
    public static string Invoicing_IssueDate => ResourceManager.GetString("Invoicing_IssueDate", resourceCulture) ?? "Invoicing_IssueDate";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_DueDate'.
    /// </summary>
    public static string Invoicing_DueDate => ResourceManager.GetString("Invoicing_DueDate", resourceCulture) ?? "Invoicing_DueDate";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Net'.
    /// </summary>
    public static string Invoicing_Net => ResourceManager.GetString("Invoicing_Net", resourceCulture) ?? "Invoicing_Net";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Vat'.
    /// </summary>
    public static string Invoicing_Vat => ResourceManager.GetString("Invoicing_Vat", resourceCulture) ?? "Invoicing_Vat";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Total'.
    /// </summary>
    public static string Invoicing_Total => ResourceManager.GetString("Invoicing_Total", resourceCulture) ?? "Invoicing_Total";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_IssueButton'.
    /// </summary>
    public static string Invoicing_IssueButton => ResourceManager.GetString("Invoicing_IssueButton", resourceCulture) ?? "Invoicing_IssueButton";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Status_Draft'.
    /// </summary>
    public static string Invoicing_Status_Draft => ResourceManager.GetString("Invoicing_Status_Draft", resourceCulture) ?? "Invoicing_Status_Draft";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Status_Issued'.
    /// </summary>
    public static string Invoicing_Status_Issued => ResourceManager.GetString("Invoicing_Status_Issued", resourceCulture) ?? "Invoicing_Status_Issued";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Status_Paid'.
    /// </summary>
    public static string Invoicing_Status_Paid => ResourceManager.GetString("Invoicing_Status_Paid", resourceCulture) ?? "Invoicing_Status_Paid";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Status_Canceled'.
    /// </summary>
    public static string Invoicing_Status_Canceled => ResourceManager.GetString("Invoicing_Status_Canceled", resourceCulture) ?? "Invoicing_Status_Canceled";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Details'.
    /// </summary>
    public static string Invoicing_Details => ResourceManager.GetString("Invoicing_Details", resourceCulture) ?? "Invoicing_Details";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Items'.
    /// </summary>
    public static string Invoicing_Items => ResourceManager.GetString("Invoicing_Items", resourceCulture) ?? "Invoicing_Items";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Item_Description'.
    /// </summary>
    public static string Invoicing_Item_Description => ResourceManager.GetString("Invoicing_Item_Description", resourceCulture) ?? "Invoicing_Item_Description";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Item_Qty'.
    /// </summary>
    public static string Invoicing_Item_Qty => ResourceManager.GetString("Invoicing_Item_Qty", resourceCulture) ?? "Invoicing_Item_Qty";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Item_Price'.
    /// </summary>
    public static string Invoicing_Item_Price => ResourceManager.GetString("Invoicing_Item_Price", resourceCulture) ?? "Invoicing_Item_Price";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Item_VatRate'.
    /// </summary>
    public static string Invoicing_Item_VatRate => ResourceManager.GetString("Invoicing_Item_VatRate", resourceCulture) ?? "Invoicing_Item_VatRate";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Item_Total'.
    /// </summary>
    public static string Invoicing_Item_Total => ResourceManager.GetString("Invoicing_Item_Total", resourceCulture) ?? "Invoicing_Item_Total";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_Subtotal'.
    /// </summary>
    public static string Invoicing_Subtotal => ResourceManager.GetString("Invoicing_Subtotal", resourceCulture) ?? "Invoicing_Subtotal";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_VatTotal'.
    /// </summary>
    public static string Invoicing_VatTotal => ResourceManager.GetString("Invoicing_VatTotal", resourceCulture) ?? "Invoicing_VatTotal";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_GrandTotal'.
    /// </summary>
    public static string Invoicing_GrandTotal => ResourceManager.GetString("Invoicing_GrandTotal", resourceCulture) ?? "Invoicing_GrandTotal";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_SwissSection'.
    /// </summary>
    public static string Invoicing_SwissSection => ResourceManager.GetString("Invoicing_SwissSection", resourceCulture) ?? "Invoicing_SwissSection";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_ScorReference'.
    /// </summary>
    public static string Invoicing_ScorReference => ResourceManager.GetString("Invoicing_ScorReference", resourceCulture) ?? "Invoicing_ScorReference";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_QrIban'.
    /// </summary>
    public static string Invoicing_QrIban => ResourceManager.GetString("Invoicing_QrIban", resourceCulture) ?? "Invoicing_QrIban";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_ItalianSection'.
    /// </summary>
    public static string Invoicing_ItalianSection => ResourceManager.GetString("Invoicing_ItalianSection", resourceCulture) ?? "Invoicing_ItalianSection";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_SdiCode'.
    /// </summary>
    public static string Invoicing_SdiCode => ResourceManager.GetString("Invoicing_SdiCode", resourceCulture) ?? "Invoicing_SdiCode";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_PecEmail'.
    /// </summary>
    public static string Invoicing_PecEmail => ResourceManager.GetString("Invoicing_PecEmail", resourceCulture) ?? "Invoicing_PecEmail";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_NoSelection'.
    /// </summary>
    public static string Invoicing_NoSelection => ResourceManager.GetString("Invoicing_NoSelection", resourceCulture) ?? "Invoicing_NoSelection";

    /// <summary>
    /// Looks up a localized string similar to key 'Invoicing_LoadedCount'.
    /// </summary>
    public static string Invoicing_LoadedCount => ResourceManager.GetString("Invoicing_LoadedCount", resourceCulture) ?? "Invoicing_LoadedCount";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Title'.
    /// </summary>
    public static string Accounting_Title => ResourceManager.GetString("Accounting_Title", resourceCulture) ?? "Accounting_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_ChartOfAccounts'.
    /// </summary>
    public static string Accounting_ChartOfAccounts => ResourceManager.GetString("Accounting_ChartOfAccounts", resourceCulture) ?? "Accounting_ChartOfAccounts";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_GeneralLedger'.
    /// </summary>
    public static string Accounting_GeneralLedger => ResourceManager.GetString("Accounting_GeneralLedger", resourceCulture) ?? "Accounting_GeneralLedger";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Code'.
    /// </summary>
    public static string Accounting_Code => ResourceManager.GetString("Accounting_Code", resourceCulture) ?? "Accounting_Code";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_AccountName'.
    /// </summary>
    public static string Accounting_AccountName => ResourceManager.GetString("Accounting_AccountName", resourceCulture) ?? "Accounting_AccountName";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Type'.
    /// </summary>
    public static string Accounting_Type => ResourceManager.GetString("Accounting_Type", resourceCulture) ?? "Accounting_Type";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Voucher'.
    /// </summary>
    public static string Accounting_Voucher => ResourceManager.GetString("Accounting_Voucher", resourceCulture) ?? "Accounting_Voucher";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_PostingDate'.
    /// </summary>
    public static string Accounting_PostingDate => ResourceManager.GetString("Accounting_PostingDate", resourceCulture) ?? "Accounting_PostingDate";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Description'.
    /// </summary>
    public static string Accounting_Description => ResourceManager.GetString("Accounting_Description", resourceCulture) ?? "Accounting_Description";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Debit'.
    /// </summary>
    public static string Accounting_Debit => ResourceManager.GetString("Accounting_Debit", resourceCulture) ?? "Accounting_Debit";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Credit'.
    /// </summary>
    public static string Accounting_Credit => ResourceManager.GetString("Accounting_Credit", resourceCulture) ?? "Accounting_Credit";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Balanced'.
    /// </summary>
    public static string Accounting_Balanced => ResourceManager.GetString("Accounting_Balanced", resourceCulture) ?? "Accounting_Balanced";

    /// <summary>
    /// Looks up a localized string similar to key 'Accounting_Balance'.
    /// </summary>
    public static string Accounting_Balance => ResourceManager.GetString("Accounting_Balance", resourceCulture) ?? "Accounting_Balance";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Title'.
    /// </summary>
    public static string Contacts_Title => ResourceManager.GetString("Contacts_Title", resourceCulture) ?? "Contacts_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Name'.
    /// </summary>
    public static string Contacts_Name => ResourceManager.GetString("Contacts_Name", resourceCulture) ?? "Contacts_Name";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Type'.
    /// </summary>
    public static string Contacts_Type => ResourceManager.GetString("Contacts_Type", resourceCulture) ?? "Contacts_Type";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Customer'.
    /// </summary>
    public static string Contacts_Customer => ResourceManager.GetString("Contacts_Customer", resourceCulture) ?? "Contacts_Customer";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Supplier'.
    /// </summary>
    public static string Contacts_Supplier => ResourceManager.GetString("Contacts_Supplier", resourceCulture) ?? "Contacts_Supplier";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Both'.
    /// </summary>
    public static string Contacts_Both => ResourceManager.GetString("Contacts_Both", resourceCulture) ?? "Contacts_Both";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_TaxCode'.
    /// </summary>
    public static string Contacts_TaxCode => ResourceManager.GetString("Contacts_TaxCode", resourceCulture) ?? "Contacts_TaxCode";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_VatNumber'.
    /// </summary>
    public static string Contacts_VatNumber => ResourceManager.GetString("Contacts_VatNumber", resourceCulture) ?? "Contacts_VatNumber";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Email'.
    /// </summary>
    public static string Contacts_Email => ResourceManager.GetString("Contacts_Email", resourceCulture) ?? "Contacts_Email";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Phone'.
    /// </summary>
    public static string Contacts_Phone => ResourceManager.GetString("Contacts_Phone", resourceCulture) ?? "Contacts_Phone";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_City'.
    /// </summary>
    public static string Contacts_City => ResourceManager.GetString("Contacts_City", resourceCulture) ?? "Contacts_City";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Country'.
    /// </summary>
    public static string Contacts_Country => ResourceManager.GetString("Contacts_Country", resourceCulture) ?? "Contacts_Country";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_Address'.
    /// </summary>
    public static string Contacts_Address => ResourceManager.GetString("Contacts_Address", resourceCulture) ?? "Contacts_Address";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_PostalCode'.
    /// </summary>
    public static string Contacts_PostalCode => ResourceManager.GetString("Contacts_PostalCode", resourceCulture) ?? "Contacts_PostalCode";

    /// <summary>
    /// Looks up a localized string similar to key 'Contacts_NewContact'.
    /// </summary>
    public static string Contacts_NewContact => ResourceManager.GetString("Contacts_NewContact", resourceCulture) ?? "Contacts_NewContact";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Title'.
    /// </summary>
    public static string Products_Title => ResourceManager.GetString("Products_Title", resourceCulture) ?? "Products_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Sku'.
    /// </summary>
    public static string Products_Sku => ResourceManager.GetString("Products_Sku", resourceCulture) ?? "Products_Sku";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Name'.
    /// </summary>
    public static string Products_Name => ResourceManager.GetString("Products_Name", resourceCulture) ?? "Products_Name";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Category'.
    /// </summary>
    public static string Products_Category => ResourceManager.GetString("Products_Category", resourceCulture) ?? "Products_Category";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Price'.
    /// </summary>
    public static string Products_Price => ResourceManager.GetString("Products_Price", resourceCulture) ?? "Products_Price";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Currency'.
    /// </summary>
    public static string Products_Currency => ResourceManager.GetString("Products_Currency", resourceCulture) ?? "Products_Currency";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Stock'.
    /// </summary>
    public static string Products_Stock => ResourceManager.GetString("Products_Stock", resourceCulture) ?? "Products_Stock";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_VatRate'.
    /// </summary>
    public static string Products_VatRate => ResourceManager.GetString("Products_VatRate", resourceCulture) ?? "Products_VatRate";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_Uom'.
    /// </summary>
    public static string Products_Uom => ResourceManager.GetString("Products_Uom", resourceCulture) ?? "Products_Uom";

    /// <summary>
    /// Looks up a localized string similar to key 'Products_NewProduct'.
    /// </summary>
    public static string Products_NewProduct => ResourceManager.GetString("Products_NewProduct", resourceCulture) ?? "Products_NewProduct";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_Title'.
    /// </summary>
    public static string Payroll_Title => ResourceManager.GetString("Payroll_Title", resourceCulture) ?? "Payroll_Title";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_EmployeeNumber'.
    /// </summary>
    public static string Payroll_EmployeeNumber => ResourceManager.GetString("Payroll_EmployeeNumber", resourceCulture) ?? "Payroll_EmployeeNumber";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_FullName'.
    /// </summary>
    public static string Payroll_FullName => ResourceManager.GetString("Payroll_FullName", resourceCulture) ?? "Payroll_FullName";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_Department'.
    /// </summary>
    public static string Payroll_Department => ResourceManager.GetString("Payroll_Department", resourceCulture) ?? "Payroll_Department";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_Role'.
    /// </summary>
    public static string Payroll_Role => ResourceManager.GetString("Payroll_Role", resourceCulture) ?? "Payroll_Role";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_GrossSalary'.
    /// </summary>
    public static string Payroll_GrossSalary => ResourceManager.GetString("Payroll_GrossSalary", resourceCulture) ?? "Payroll_GrossSalary";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_MonthlyNet'.
    /// </summary>
    public static string Payroll_MonthlyNet => ResourceManager.GetString("Payroll_MonthlyNet", resourceCulture) ?? "Payroll_MonthlyNet";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_SocialSecurity'.
    /// </summary>
    public static string Payroll_SocialSecurity => ResourceManager.GetString("Payroll_SocialSecurity", resourceCulture) ?? "Payroll_SocialSecurity";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_HireDate'.
    /// </summary>
    public static string Payroll_HireDate => ResourceManager.GetString("Payroll_HireDate", resourceCulture) ?? "Payroll_HireDate";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_PreviewButton'.
    /// </summary>
    public static string Payroll_PreviewButton => ResourceManager.GetString("Payroll_PreviewButton", resourceCulture) ?? "Payroll_PreviewButton";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_TaxDeductions'.
    /// </summary>
    public static string Payroll_TaxDeductions => ResourceManager.GetString("Payroll_TaxDeductions", resourceCulture) ?? "Payroll_TaxDeductions";

    /// <summary>
    /// Looks up a localized string similar to key 'Payroll_NewEmployee'.
    /// </summary>
    public static string Payroll_NewEmployee => ResourceManager.GetString("Payroll_NewEmployee", resourceCulture) ?? "Payroll_NewEmployee";
}
