// <copyright file="Product.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Model;

public class Product(int id, string name, decimal unitPrice)
{
    public Product() : this(-1, null, -1) { }
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public decimal UnitPrice { get; set; } = unitPrice;
    public override string ToString()
    {
        return Name ?? string.Empty;
    }
}