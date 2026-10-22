using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DevExpress.Data.Utils;
using DevExpress.Export;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace AIDemo.ViewModels {
    [POCOViewModel]
    public class SmartPasteModuleViewModel {
        public ObservableCollection<PasteMode> PasteModes { get; } = new ObservableCollection<PasteMode>() {
            PasteMode.Append,
            PasteMode.Update,
        };

        public ObservableCollection<Record> Items { get; } = new ObservableCollection<Record>(DataHelper.GetData(10));
        public ObservableCollection<string> Companies { get; } = new ObservableCollection<string>(DataHelper.companies.ToList());

        public string GridCopyText { get; set; } =
            "Transactions: \n" +
            "- Hanari Carnes - Invoice amount due: $123.00, due by 10/30/2024. Status: Processed ✅ \n" +
            "- Que Delícia - Amount: $231.00 | Due date: 10/29/2024 | Not processed yet \n" +
            "- Romero y tomillo - $369.00 required by 10/28/2024, Processed ✅ \n" +
            "- Mère Paillard - Payment: $158.00, due: 10/31/2024, unprocessed \n" +
            "- Eastern Delights has an outstanding balance of $97.00, which was due on 10/27/2024. Status: Processed ✅ \n";
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

        public DelegateCommand CopyTextCommand { get; }

        public SmartPasteModuleViewModel() {
            CopyTextCommand = new DelegateCommand(() => {
                SafeClipboardWpf.Instance.SetText(LayoutCopyText);
            }, () => {
                return LayoutCopyText.Length > 0;
            });
        }
    }

    public class Record : BindableBase {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public decimal? Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool Processed { get; set; }

        public Record() { }

        public override string ToString() {
            return string.Format("ID = {0}, Text = {1}", ID, CompanyName);
        }
    }

    public class DataHelper {
        public static string[] companies = new string[] {
            "Hanari Carnes",
            "Que Delícia",
            "Romero y tomillo",
            "Mère Paillarde",
            "Comércio Mineiro",
            "Reggiani Caseifici",
            "Maison Dewey"
        };

        public static string[] items = new string[] {
            "Office Chairs",
            "Laptops",
            "Printer Ink",
            "Tables",
            "Paper"
        };

        public static BindingList<Record> GetData(int count) {
            BindingList<Record> records = new BindingList<Record>();
            var rnd = DevExpress.Data.Utils.NonCryptographicRandom.Default;
            for(int i = 0; i < count; i++) {
                int n = rnd.Next(10);
                var record = new Record() {
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
}
