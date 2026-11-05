using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

using FatturaFacile.Wpf;
using FatturaFacile.Wpf.Contracts.Services;
using FatturaFacile.Wpf.Helpers;
using FatturaFacile.Wpf.Models;

using Syncfusion.Windows.Shared;

namespace FatturaFacile.Wpf.ViewModels
{
    public class TileViewViewModel : Observable
    {
        private ObservableCollection<BookModel> bookModelItems;
        public ObservableCollection<BookModel> BookModelItems
        {
            get
            {
                return bookModelItems;
            }

            set
            {
                bookModelItems = value;
            }
        }

        public TileViewViewModel()
        {
            bookModelItems = new ObservableCollection<BookModel>();
            PopulateCollection();
        }

        private void PopulateCollection()
        {
            String path = AppDomain.CurrentDomain.BaseDirectory;
            path = path + "Assets/Books.xml";
            XDocument xDocument = XDocument.Load(path);
            IEnumerable<XElement> query = from xElement in xDocument.Descendants("book")
                                          select xElement;
            foreach (XElement xElement in query)
            {
                BookModel bookModel = new BookModel
                {
                    BookID = xElement.FirstAttribute.Value,
                    BookName = xElement.Element("title").Value,
                    AuthorName = xElement.Element("author").Value,
                    Genre = xElement.Element("genre").Value,
                    Price = xElement.Element("price").Value,
                    PublishDate = xElement.Element("publish_date").Value,
                    Description = xElement.Element("description").Value
                };
                bookModelItems.Add(bookModel);
            }
        }
    }
}
