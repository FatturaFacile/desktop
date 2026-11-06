// <copyright file="SavePurchaseInvoiceCommandValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FluentValidation;

namespace FatturaFacile.Application.Features.Purchasing;

/// <summary>
/// Validator for <see cref="SavePurchaseInvoiceCommand"/>.
/// </summary>
public sealed class SavePurchaseInvoiceCommandValidator : AbstractValidator<SavePurchaseInvoiceCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SavePurchaseInvoiceCommandValidator"/> class.
    /// </summary>
    public SavePurchaseInvoiceCommandValidator()
    {
        _ = RuleFor(x => x.Invoice.SupplierId).NotEqual(Guid.Empty).WithMessage("Selezionare il fornitore.");
        _ = RuleFor(x => x.Invoice.DocumentNumber).NotEmpty().WithMessage("Indicare il numero del documento del fornitore.");
        _ = RuleFor(x => x.Lines).NotEmpty().WithMessage("Inserire almeno una riga.");
        _ = RuleForEach(x => x.Lines).ChildRules(static line =>
        {
            _ = line.RuleFor(l => l.Description).NotEmpty().WithMessage("Descrizione della riga mancante.");
            _ = line.RuleFor(l => l.VatRate).InclusiveBetween(0m, 30m).WithMessage("Aliquota IVA non valida.");
            _ = line.RuleFor(l => l.DeductiblePercent).InclusiveBetween(0m, 100m).WithMessage("Percentuale di detrazione non valida.");
        });
    }
}
