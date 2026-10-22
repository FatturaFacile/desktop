// <copyright file="ISupportFiltering.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#if NET
using DevExpress.AIIntegration.Extensions;
#endif

namespace FatturaFacile.Filtering.ViewModel; 
public interface ISupportFiltering<TEntity> : ISupportFiltering where TEntity : class {
    Expression<Func<TEntity, bool>> FilterExpression { get; set; }
}

public interface ISupportFiltering {
    string FilterString { get; set; }
#if NET
    IEnumerable<PromptToExpressionRequestBase.ColumnInfo> GetMetadata();
#endif

}
