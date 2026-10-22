// <copyright file="IDocumentModule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace Common;

public interface IDocumentModule
{
    string Caption { get; }
    bool IsActive { get; set; }
}
