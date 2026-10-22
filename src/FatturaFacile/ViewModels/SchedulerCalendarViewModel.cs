using System;
using System.Collections.ObjectModel;
using System.Linq;

using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;

using FatturaFacile.Infrastructure;
using FatturaFacile.Model;

namespace FatturaFacile.ViewModels;
//Properties and commands are generated automatically for the following class when compiling:
//Refer to the following help topic for details: https://docs.devexpress.com/WPF/402989/mvvm-framework/viewmodels/compile-time-generated-viewmodels
[GenerateViewModel]
public partial class SchedulerCalendarViewModel(ICommonDataService dataService)
{
    ICommonDataService DataService = dataService;
    bool isInitialized;
    [GenerateProperty]
    ObservableCollection<AppointmentData> appointmentsSource;

    [GenerateProperty]
    ObservableCollection<CalendarResouce> calendars;

    [GenerateProperty]
    DateTimeRange selectedInterval = new DateTimeRange(DateTime.Now, DateTime.Now.AddHours(1));

    public SchedulerCalendarViewModel() : this(null) { }

    [GenerateCommand]
    void Init()
    {
        if (isInitialized)
            return;
        AppointmentsSource = new(DataService.GetAppointmentData());
        Calendars = new(DataService.GetCalendarResources());
        isInitialized = true;
    }
}
