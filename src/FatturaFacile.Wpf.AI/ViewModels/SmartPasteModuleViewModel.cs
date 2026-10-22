// <copyright file="SmartPasteModuleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;

using DevExpress.Data.Utils;
using DevExpress.Export;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace FatturaFacile.ViewModels;

/// <summary>
/// ViewModel demonstrating AI-assisted smart paste into grids and layout controls.
/// </summary>
[POCOViewModel]
public class SmartPasteModuleViewModel
{
    /// <summary>
    /// Gets the selectable paste modes (e.g., Append vs. Update) supported in the demonstration.
    /// </summary>
    public ObservableCollection<PasteMode> PasteModes { get; } = new ObservableCollection<PasteMode>()
    {
        PasteMode.Append,
        PasteMode.Update,
    };

    /// <summary>
    /// Gets the collection of parsed transaction records bound to the data grid.
    /// </summary>
    public ObservableCollection<Record> Items { get; } = new ObservableCollection<Record>(DataHelper.GetData(10));

    /// <summary>
    /// Gets the list of predefined company names used for combo box selection.
    /// </summary>
    public ObservableCollection<string> Companies { get; } =
        new ObservableCollection<string>(DataHelper.companies.ToList());

    /// <summary>
    /// Gets or sets unformatted sample text representing tabular transaction lines ready for clipboard copying.
    /// </summary>
    public string GridCopyText { get; set; } =
        "Transactions: \n" +
        "- Hanari Carnes - Invoice amount due: $123.00, due by 10/30/2024. Status: Processed ✅ \n" +
        "- Que Delícia - Amount: $231.00 | Due date: 10/29/2024 | Not processed yet \n" +
        "- Romero y tomillo - $369.00 required by 10/28/2024, Processed ✅ \n" +
        "- Mère Paillard - Payment: $158.00, due: 10/31/2024, unprocessed \n" +
        "- Eastern Delights has an outstanding balance of $97.00, which was due on 10/27/2024. Status: Processed ✅ \n";

    /// <summary>
    /// Gets or sets unstructured form-like text designed for pasting into a layout control.
    /// </summary>
    public string LayoutCopyText { get; set; } =
        "Payment: Amount - $123.00 \n" +
        "Statement Date: 10/15/2024 \n" +
        "Account Number: 133244556 \n" +
        "Name: John Smith \n" +
        "Contact: (123) 456-7890 \n" +
        "Email: john@myemail.com \n" +
        "Address: \n" +
        "- 123 Elm St Apt 4B \n" +
        "- New York, NY 10001 \n";

    /// <summary>
    /// Gets the command that copies sample text into the system clipboard.
    /// </summary>
    public DelegateCommand CopyTextCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartPasteModuleViewModel"/> class.
    /// </summary>
    public SmartPasteModuleViewModel()
    {
        CopyTextCommand = new DelegateCommand(
            () => { _ = SafeClipboardWpf.Instance.SetText(LayoutCopyText); }, () => { return LayoutCopyText.Length > 0; });
    }
}

/// <summary>
/// Demonstration transaction record for smart paste operations.
/// </summary>
public class Record : BindableBase
{
    /// <summary>Gets or sets the record transaction identifier.</summary>
    public int ID { get; set; }

    /// <summary>Gets or sets the corporate or client account name.</summary>
    public string CompanyName { get; set; }

    /// <summary>Gets or sets the monetary invoice or transaction amount.</summary>
    public decimal? Amount { get; set; }

    /// <summary>Gets or sets the scheduled payment deadline or maturity date.</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the transaction was reconciled and processed.</summary>
    public bool Processed { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Record"/> class.
    /// </summary>
    public Record()
    {
    }

    /// <inheritdoc />
    public override string ToString() => string.Format("ID = {0}, Text = {1}", ID, CompanyName);
}

/// <summary>
/// Helper class providing sample company and record datasets.
/// </summary>
public class DataHelper
{
    /// <summary>Sample vendor and company names.</summary>
    public static string[] companies = new string[]
    {
        "Hanari Carnes",
        "Que Delícia",
        "Romero y tomillo",
        "Mère Paillarde",
        "Comércio Mineiro",
        "Reggiani Caseifici",
        "Maison Dewey"
    };

    /// <summary>Sample catalog supply items.</summary>
    public static string[] items = new string[]
    {
        "Office Chairs",
        "Laptops",
        "Printer Ink",
        "Tables",
        "Paper"
    };

    /// <summary>
    /// Generates a synthesized collection of transaction records for testing clipboard parsing.
    /// </summary>
    /// <param name="count">The requested quantity of transaction records.</param>
    /// <returns>A data-bindable list of populated <see cref="Record"/> instances.</returns>
    public static BindingList<Record> GetData(
        int count)
    {
        var records = new BindingList<Record>();
        var rnd = NonCryptographicRandom.Default;
        for (var i = 0; i < count; i++)
        {
            var n = rnd.Next(10);
            var record = new Record()
            {
                ID = i + 100,
                CompanyName = companies[i % companies.Length],
                DueDate = DateTime.Today.AddDays(n - 5),
                Amount = i % 2 == 0 ? (i + 1) * 123 : i * 231,
                Processed = i % 2 == 0,
            };
            records.Add(record);
        }

        return records;
    }
}
