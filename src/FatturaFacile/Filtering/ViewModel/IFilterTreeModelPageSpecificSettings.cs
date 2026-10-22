// <copyright file="IFilterTreeModelPageSpecificSettings.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FatturaFacile.Filtering.ViewModel;

public interface IFilterTreeModelPageSpecificSettings {
    string StaticFiltersTitle { get; }
    string CustomFiltersTitle { get; }
    FilterInfoList StaticFilters { get; set; }
    FilterInfoList CustomFilters { get; set; }
    IEnumerable<string> HiddenFilterProperties { get; }
    IEnumerable<string> AdditionalFilterProperties { get; }
    void SaveSettings();
}
