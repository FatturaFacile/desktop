// <copyright file="DataService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using FatturaFacile.Model;

namespace FatturaFacile.Infrastructure;

public class DataService : IDataService
{
    public async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        await Task.Delay(300);
        return Enumerable.Range(1, 50).Select(x => new Customer(x)).ToList();
    }
}

public interface IDataService
{
    Task<IEnumerable<Customer>> GetCustomersAsync();
}

public class DataService : ICommonDataService
{
    List<Customer> customers = [
        new(1, "John", "Doe", "USA", "Homesville", "123 Home Lane", "+1 (323) 555-4321", "55555"),
        new(2, "Sam", "Hill", "Germany", "Berlin", "Friedrichstraße 123", "+49 (30) 1234-5678", "10117"),
        new(3, "Karen", "Holmes", "USA", "Hillsville", "45 Hill St.", "+1 (212) 555-7890", "44444"),
        new(4, "Bobbie", "Valentine", "Germany", "Munich", "Maximilianstraße 45", "+49 (89) 9876-5432", "80539"),
        new(5, "Jennie", "Fuller", "Germany", "Hamburg", "Reeperbahn 89", "+49 (40) 2233-4455", "20359"),
        new(6, "Albert", "Menendez", "USA", "Chicago", "933 Heart St. Suite", "+1 (312) 555-6789", "9900"),
        new(7, "Frank", "Frankson", "France", "Paris", "25 Rue de Rivoli", "+33 (1) 23-45-67-89", "75004"),
        new(8, "Christa", "Christie", "UK", "London", "221B Baker Street", "+44 (20) 7946-0958", "NW1 6XE"),
        new(9, "Jimmie", "Jones", "USA", "Newman", "900 Newman Center.", "+1 (312) 555-6789", "89123"),
        new(10, "Alfred", "Newman", "Germany", "Frankfurt", "Goethestraße 10", "+49 (69) 1122-3344", "60313"),
        new(11, "Benjamin", "Johnson", "UK", "Manchester", " 50 Deansgate", "+44 (161) 555-6789", "M3 2BW"),
        new(12, "Alex", "James", "USA", "Los Angeles", "349 Graphic Design L", "+1 (305) 555-9876", "12211"),
        new(13, "Beau", "Alessandro", "UK", "Birmingham", "30 New Street", "+44 (121) 555-4321", "B2 4ND"),
        new(14, "Bruce", "Cambell", "USA", "San Francisco", "350 Market Street", "+1 (415) 555-1234", "94103"),
        new(15, "Cindy", "Haneline", "USA", "Houston", "600 Main Street", "+1 (713) 555-5678", "77002"),
        new(16, "Andrea", "Deville", "France", "Bordeaux", "14 Cours de l'Intendance", "+33 (5) 56-78-90-12", "33000"),
        new(17, "Anita", "Ryan", "UK", "Cardiff", "30 Castle Street", "+44 (29) 555-1234", "CF10 1BT"),
        new(18, "George", "Bunkelman", "UK", "Liverpool", "100 Albert Dock", "+44 (151) 555-6789", "L3 4AA"),
        new(19, "Anita", "Cardle", "USA", "Seattle", "1200 Pine Street", "+1 (206) 555-8765", "98101"),
        new(20, "Andrew", "Carter", "Germany", "Stuttgart", "Königstraße 20", "+49 (711) 6543-2100", "70173"),
        new(21, "Almas", "Bunch", "USA", "Boston", "75 Beacon Street", "+1 (617) 555-4321", "02108"),
        new(22, "Abigail", "Hazel", "UK", "Glasgow", "75 Buchanan Street", "+44 (141) 555-4321", "G1 3HL"),
        new(23, "Anthony", "Vicars", "Germany", "Düsseldorf", "Königsallee 85", "+49 (211) 7894-5678", "40212"),
        new(24, "Dora", "Geeter", "UK", "Bristol", "20 Queen Square", "+44 (117) 555-8765", "BS1 4ND"),
        new(24, "Anthony", "Boyd", "USA", "Denver", "1500 Blake Street", "+1 (303) 555-7890", "80202"),
        new(24, "Dora", "Catto", "USA", "Atlanta", "200 Peachtree Street", "+1 (404) 555-2345", "30303"),
    ];
    List<Product> products = [
        new(101, "Zaanse koeken", 9.5m),
        new(102, "Teatime Chocolate Biscuits", 9.2m),
        new(103, "Sir Rodney's Scones", 10.0m),
        new(104, "Filo Mix", 7.0m),
        new(105, "Tourtière", 7.45m),
        new(106, "Nord-Ost Matjeshering", 25.89m),
        new(107, "Pâté chinois", 24.0m),
        new(108, "Gnocchi di nonna Alice", 38.0m),
        new(109, "Sasquatch Ale", 14.0m),
        new(110, "Gravad lax", 26.0m),
    ];
    List<Order> orders;

    public DataService()
    {
        orders = GenerateOrders(150);
    }
    public IEnumerable<Customer> GetCustomers() => customers;
    public IEnumerable<Product> GetProducts() => products;
    public IEnumerable<Order> GetOrders() => orders;
    private List<Order> GenerateOrders(int count)
    {
        Random random = new Random();
        var generatedOrders = new List<Order>();
        for (int i = 1; i <= count; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var order = new Order { Id = i, Customer = customer, OrderDate = DateTime.Now.AddDays(-random.Next(1, 500)), Items = new ObservableCollection<OrderItem>() };

            int itemsCount = random.Next(1, 5);
            for (int j = 0; j < itemsCount; j++)
            {
                var product = products[random.Next(products.Count)];
                order.Items.Add(new OrderItem { Id = i * 10 + j, Order = order, Product = product, Quantity = random.Next(1, 10) });
            }

            customer.Orders.Add(order);
            generatedOrders.Add(order);
        }

        return generatedOrders;
    }
    List<CalendarResouce> сalendarResources = [
        new(0, "Work", "My Calendars", true),
        new(1, "Home", "My Calendars", true),
        new(2, "Room 305", "Rooms", false),
        new(3, "Room 401", "Rooms", false),
    ];
    public IEnumerable<CalendarResouce> GetCalendarResources() => сalendarResources;
    public IEnumerable<AppointmentData> GetAppointmentData()
    {
        List<AppointmentData> apts = new List<AppointmentData>();
        DateTime startGenDate = DateTime.Now.AddDays(-3);
        Random rndHour = new Random();
        for (int i = 1; i < 80; i++)
        {
            int hourAdd = rndHour.Next(3, 10);
            apts.Add(new AppointmentData(id: 0, startTime: startGenDate.AddHours(i * 5 + hourAdd + 3), endTime: startGenDate.AddHours(i * 5 + hourAdd + 4), subject: "Appointment" + i, notes: "Appointment Notes" + i, resourceId: сalendarResources[i % сalendarResources.Count].Id, labelId: 0, statusId: i % 3, recurrenceInfo: null));
        }

        return apts;
    }
    public IEnumerable<CompanyUser> GetCompanyUsers() => [
        new CompanyUser(109, 0, "Bruce", "Cambell", "Chief Executive Officer", "Executive General and Administration", "Bruce_Cambell@example.com", "(417) 166-3268"),
        new CompanyUser(101, 109, "Ken", "Sanchez", "Vice President", "Executive General and Administration", "Ken_Sanchez@example.com", "(214) 126-8555"),
        new CompanyUser(102, 101, "Brian", "Welcker", "Vice President", "Executive General and Administration", "Brian_Welcker@example.com", "(305) 654-4417"),
        new CompanyUser(103, 101, "David", "Bradley", "Vice President of R&D", "Executive General and Administration", "David_Bradley@example.com", "(702) 649-5647"),
        new CompanyUser(104, 103, "Jillian", "Carson", "Engineering Manager", "Engineering", "Jillian_Carson@example.com", "(303) 376-7233"),
        new CompanyUser(105, 104, "Michael", "Sullivan", "Senior Engineer", "Engineering", "Michael_Sullivan@example.com", "(704) 522-7625"),
        new CompanyUser(106, 104, "Linda", "Mitchell", "Senior Engineer", "Engineering", "Linda_Mitchell@example.com", "(509) 169-2345"),
        new CompanyUser(107, 102, "Frank", "Richards", "Director of Sales", "Sales", "Frank_Richards@example.com", "(704) 827-5432"),
        new CompanyUser(108, 107, "James", "Hamilton", "Sales Manager", "Sales", "James_Hamilton@example.com", "(724) 247-3834"),
        new CompanyUser(263, 103, "Rachel", "Scott", "Senior Tool Designer", "Design", "Rachel_Scott@example.com", "(253) 371-7165"),
        new CompanyUser(5, 263, "Harold", "Smodey", "Tool Designer", "Design", "Harold_Smodey@example.com", "(704) 272-1178"),
        new CompanyUser(265, 107, "Barbara", "Chinavare", "Sales Manager", "Sales", "Barbara_Chinavare@example.com", "(408) 683-2356"),
        new CompanyUser(11, 263, "Charles", "Stender", "Design Engineer", "Design", "Charles_Stender@example.com", "(345) 113-5578"),
        new CompanyUser(4, 263, "Donald", "Bradley", "Senior Tool Designer", "Design", "Donald_Bradley@example.com", "(568) 557-1001"),
        new CompanyUser(270, 102, "Alex", "Melton", "Finance Manager", "Executive General and Administration", "Alex_Melton@example.com", "(109) 367-7554"),
        new CompanyUser(217, 107, "Allison", "Etter", "Sales Representative", "Sales", "Allison_Etter@example.com", "(643) 543-5687"),
        new CompanyUser(79, 104, "Archie", "Matese", "Software Engineer", "Engineering", "Archie_Matese@example.com", "(875) 887-0287"),
        new CompanyUser(114, 104, "Arvil", "Chase", "Software Engineer", "Engineering", "Arvil_Chase@example.com", "(467) 343-0037")
    ];
    public IEnumerable<CountryData> GetCountryData() => [
        new CountryData(1, 0, "Norway", 385207),
        new CountryData(2, 0, "Sweden", 528447),
        new CountryData(3, 0, "Denmark", 42951),
        new CountryData(4, 0, "Finland", 338455),
        new CountryData(5, 0, "Iceland", 103000),
        new CountryData(6, 0, "Ireland", 84421),
        new CountryData(7, 0, "United Kingdom", 243610),
        new CountryData(18, 17, "Spain", 505990),
        new CountryData(19, 17, "Portugal", 92212),
        new CountryData(20, 17, "Greece", 131957),
        new CountryData(21, 17, "Italy", 301230),
        new CountryData(22, 17, "Malta", 316),
        new CountryData(23, 17, "San Marino", 61.2),
        new CountryData(25, 17, "Serbia", 88499),
        new CountryData(27, 26, "USA", 9522055),
        new CountryData(28, 26, "Canada", 9984670),
        new CountryData(30, 29, "Argentina", 2780400),
        new CountryData(31, 29, "Brazil", 8514215),
        new CountryData(34, 32, "India", 3287263),
        new CountryData(35, 32, "Japan", 377975),
        new CountryData(36, 32, "China", 9597000)
    ];
    public IEnumerable<KanbanGroup> GetKanbanData()
    {
        List<KanbanGroup> kanbanGroups = new List<KanbanGroup>();
        Random grItemsRnd = new Random();
        for (int grNumber = 1; grNumber < 7; grNumber++)
        {
            KanbanGroup group = new KanbanGroup("Group" + grNumber) { Items = new() };
            for (int itemNumber = 1; itemNumber < grItemsRnd.Next(1, 20); itemNumber++)
            {
                group.Items.Add(new KanbanItem($"Task{grNumber}.{itemNumber}", "Task description", (grNumber + itemNumber) % 7 == 0));
            }

            kanbanGroups.Add(group);
        }

        return kanbanGroups;
    }
    public IEnumerable<GanttTaskItem> GetGanttTasks()
    {
        var startDate = DateTime.Now.Date.AddDays(3);
        return [
            new GanttTaskItem(0, null, startDate, startDate + TimeSpan.FromDays(5), "Market Analysis", 1, Enumerable.Empty<GanttTaskLink>()),
            new GanttTaskItem(1, null, startDate + TimeSpan.FromDays(5), startDate + TimeSpan.FromDays(9), "Feature Planning", 1, [new GanttTaskLink(0)]),
            new GanttTaskItem(2, null, startDate + TimeSpan.FromDays(9), startDate + TimeSpan.FromDays(16), "Feature 1", 0, Enumerable.Empty<GanttTaskLink>()),
            new GanttTaskItem(3, 2, startDate + TimeSpan.FromDays(9), startDate + TimeSpan.FromDays(13), "Implementation", 1, [new GanttTaskLink(1)]),
            new GanttTaskItem(4, 2, startDate + TimeSpan.FromDays(13), startDate + TimeSpan.FromDays(16), "Demos & Docs", 1, [new GanttTaskLink(3)]),
            new GanttTaskItem(5, null, startDate + TimeSpan.FromDays(9), startDate + TimeSpan.FromDays(17), "Feature 2", 0.85, Enumerable.Empty<GanttTaskLink>()),
            new GanttTaskItem(6, 5, startDate + TimeSpan.FromDays(9), startDate + TimeSpan.FromDays(13), "Implementation", 1, [new GanttTaskLink(1)]),
            new GanttTaskItem(7, 5, startDate + TimeSpan.FromDays(13), startDate + TimeSpan.FromDays(17), "Demos & Docs", 0.7, [new GanttTaskLink(6)]),
            new GanttTaskItem(8, null, startDate + TimeSpan.FromDays(16), startDate + TimeSpan.FromDays(19), "Feature 3", 0.4, Enumerable.Empty<GanttTaskLink>()),
            new GanttTaskItem(9, 8, startDate + TimeSpan.FromDays(16), startDate + TimeSpan.FromDays(18), "Implementation", 0.8, [new GanttTaskLink(4)]),
            new GanttTaskItem(10, 8, startDate + TimeSpan.FromDays(18), startDate + TimeSpan.FromDays(19), "Demos & Docs", 0, [new GanttTaskLink(9)]),
            new GanttTaskItem(11, null, startDate + TimeSpan.FromDays(19), startDate + TimeSpan.FromDays(23), "Testing & Bug Fixing", 0, [new GanttTaskLink(7), new GanttTaskLink(10)]),
            new GanttTaskItem(12, null, startDate + TimeSpan.FromDays(23), startDate + TimeSpan.FromDays(23), "Development finished", 0, [new GanttTaskLink(11)])
        ];
    }
    public IEnumerable<string> GetCountries() => [
        "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina", "Armenia", "Australia", "Austria",
        "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan",
        "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia",
        "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros", "Congo (Congo-Brazzaville)", "Costa Rica",
        "Croatia", "Cuba", "Cyprus", "Czechia", "Democratic Republic of the Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador",
        "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France",
        "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau",
        "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland",
        "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kuwait", "Kyrgyzstan",
        "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar",
        "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia",
        "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal",
        "Netherlands", "New Zealand", "Nicaragua", "Niger", "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan",
        "Palau", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
        "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia",
        "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa",
        "South Korea", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Tajikistan",
        "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Tuvalu",
        "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela",
        "Vietnam", "Yemen", "Zambia", "Zimbabwe"
    ];
}

public interface ICommonDataService
{
    IEnumerable<CompanyUser> GetCompanyUsers();
    IEnumerable<Customer> GetCustomers();
    IEnumerable<Order> GetOrders();
    IEnumerable<Product> GetProducts();
    IEnumerable<GanttTaskItem> GetGanttTasks();
    IEnumerable<AppointmentData> GetAppointmentData();
    IEnumerable<CalendarResouce> GetCalendarResources();
    IEnumerable<KanbanGroup> GetKanbanData();
    IEnumerable<CountryData> GetCountryData();
    IEnumerable<string> GetCountries();
}
