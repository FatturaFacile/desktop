// <copyright file="InvoiceViewModelProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// AutoMapper configuration profile mapping invoice DTOs to WPF presentation row view models.
/// </summary>
public class InvoiceViewModelProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceViewModelProfile"/> class and configures mapping expressions.
    /// </summary>
    public InvoiceViewModelProfile()
    {
        _ = CreateMap<InvoiceListItemDto, InvoiceRowViewModel>();
        _ = CreateMap<InvoiceDto, InvoiceRowViewModel>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.ContactName));
    }
}
