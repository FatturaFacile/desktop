using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

using FatturaFacile.Wpf.Contracts.Services;
using FatturaFacile.Wpf.Helpers;
using FatturaFacile.Wpf.Models;

using Syncfusion.Windows.Shared;

namespace FatturaFacile.Wpf
{
    public class DateTimeRangeNavigatorViewModel : Observable
    {
        public ObservableCollection<DateTimeRangeNavigatorModel> UsersList { get; set; }

        public DateTimeRangeNavigatorViewModel()
        {
            this.UsersList = new ObservableCollection<DateTimeRangeNavigatorModel>();

            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2010,01,01), NoOfUsers = 3000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2011,01,01), NoOfUsers = 5000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2012,01,01), NoOfUsers = 2000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2013,01,01), NoOfUsers = 7000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2014,01,01), NoOfUsers = 6000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2015,01,01), NoOfUsers = 3000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2016,01,01), NoOfUsers = 2000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2017,01,01), NoOfUsers = 5500 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2018,01,01), NoOfUsers = 6000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2019,01,01), NoOfUsers = 3000 });
            UsersList.Add(new DateTimeRangeNavigatorModel { TimeStamp = new DateTime(2020,01,01), NoOfUsers = 4000 });

        }
    }
}
