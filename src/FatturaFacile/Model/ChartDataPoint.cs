using System.Collections.Generic;

namespace FatturaFacile.Model;

public class ChartDataPoint<TArgument, TValue>(TArgument argument, TValue value, string group = null)
{
    public TArgument Argument { get; set; } = argument;
    public TValue Value { get; set; } = value;
    public string Group { get; set; } = group;
}

public class DataSeries
{
    public string Name { get; set; }
    public IEnumerable<object> Values { get; set; }
}