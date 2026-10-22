// <copyright file="FilterInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FatturaFacile.Filtering.ViewModel;

public class FilterInfo {
    public string Name { get; set; }
    public string FilterCriteria { get; set; }
    public string ImageUri { get; set; }
}

public class FilterInfoList : List<FilterInfo> {
    public FilterInfoList() { }
    public FilterInfoList(IEnumerable<FilterInfo> filters)
        : base(filters) {
    }
}
