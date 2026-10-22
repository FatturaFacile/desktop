// <copyright file="MapViewModelBase.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.ViewModels; 
public abstract partial class MapViewModelBase {
    public const string AzureKey = "CDOhtwNBSsbmBiN3rUEjmBJGHW2tRMbp5XVwu4J55VBZg8PdRe9MJQQJ99BEACYeBjFllM6LAAAgAZMP1cA4";
    public virtual Address Address { get; set; }
}
