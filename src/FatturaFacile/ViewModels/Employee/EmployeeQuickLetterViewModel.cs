// <copyright file="EmployeeQuickLetterViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using DevExpress.DevAV.Common.ViewModel;
using DevExpress.DevAV.DevAVDbDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;

namespace FatturaFacile.ViewModels; 
public class EmployeeQuickLetterViewModel {
    public static EmployeeQuickLetterViewModel Create() {
        return ViewModelSource.Create(() => new EmployeeQuickLetterViewModel());
    }
    protected EmployeeQuickLetterViewModel() { }
    public virtual Employee Entity { get; set; }
    protected IDocumentManagerService DocumentManagerService { get { return this.GetRequiredService<IDocumentManagerService>(); } }
    public void ShowMailMerge() {
        QuickLetter(string.Empty);
    }
    public void QuickLetter(string templateName) {
        var mailMergeViewModel = MailMergeViewModel<Employee, LinksViewModel>.Create(UnitOfWorkSource.GetUnitOfWorkFactory(), x => x.Employees, Entity == null ? null as long? : Entity.Id, templateName, LinksViewModel.Create());
        DocumentManagerService.CreateDocument("EmployeeMailMergeView", mailMergeViewModel, null, this).Show();
    }
    public bool CanQuickLetter(string templateName) {
        return Entity != null;
    }
    protected virtual void OnEntityChanged() {
        this.RaiseCanExecuteChanged(x => x.QuickLetter(""));
    }
}
