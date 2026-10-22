// <copyright file="GridGroupingBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Input;

using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;

namespace FatturaFacile.Wpf.Common.Behaviors;

/// <summary>
/// Attached behavior for DevExpress <see cref="GridControl"/> that coordinates dynamic column grouping and new item command execution.
/// </summary>
public class GridGroupingBehavior : Behavior<GridControl>
{

    /// <summary>
    /// The associated table view instance.
    /// </summary>
    private TableView? tableView;

    /// <summary>
    /// Identifies the <see cref="Group"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(
                                                                                          name: "Group",
                                                                                          propertyType: typeof(string),
                                                                                          ownerType: typeof(GridGroupingBehavior),
                                                                                          typeMetadata: new FrameworkPropertyMetadata(
                                                                                                                                      defaultValue: null,
                                                                                                                                      propertyChangedCallback: (
                                                                                                                                                                       d,
                                                                                                                                                                       e) =>
                                                                                                                                                                   ((GridGroupingBehavior)
                                                                                                                                                                       d)
                                                                                                                                                                  .OnGroupChanged(
                                                                                                                                                                                  e: e)));

    /// <summary>
    /// Gets or sets the field name by which the grid rows are grouped.
    /// When <see langword="null"/> or empty, existing grouping is cleared.
    /// </summary>
    public string? Group
    {
        get => (string?)GetValue(dp: GroupProperty);
        set => SetValue(dp: GroupProperty, value: value);
    }

    /// <summary>
    /// Identifies the <see cref="NewItemCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NewItemCommandProperty = DependencyProperty.Register(
                                                                                                   name: "NewItemCommand",
                                                                                                   propertyType: typeof(ICommand),
                                                                                                   ownerType: typeof(
                                                                                                       GridGroupingBehavior),
                                                                                                   typeMetadata: new PropertyMetadata(
                                                                                                                                      propertyChangedCallback: null));

    /// <summary>
    /// Gets or sets the command executed when the new item row is focused.
    /// </summary>
    public ICommand? NewItemCommand
    {
        get => (ICommand?)GetValue(dp: NewItemCommandProperty);
        set => SetValue(dp: NewItemCommandProperty, value: value);
    }

    /// <summary>
    /// Handles changes to the Group dependency property and applies grouping to the grid.
    /// </summary>
    /// <param name="e">The event data containing the property changes.</param>
    private void OnGroupChanged(DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(value: Group))
        {
            AssociatedObject.ClearGrouping();
        }

        if (Group != null && AssociatedObject.Columns.GetColumnByFieldName(fieldName: Group) != null)
        {
            AssociatedObject.GroupBy(fieldName: Group);
        }
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.Loaded += Loaded;
        }
    }

    /// <summary>
    /// Handles the Loaded event of the AssociatedObject to initialize table view event subscriptions.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void Loaded(
        object          sender,
        RoutedEventArgs e)
    {
        if (AssociatedObject != null)
        {
            tableView = AssociatedObject.View as TableView;

            if (tableView != null)
            {
                tableView.FocusedRowHandleChanged -= FocusedRowHandleChanged;
                tableView.FocusedRowHandleChanged += FocusedRowHandleChanged;
            }
        }
    }

    /// <summary>
    /// Handles changes to the focused row handle to trigger the new item command when the new item row is focused.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    private void FocusedRowHandleChanged(
        object                           sender,
        FocusedRowHandleChangedEventArgs e)
    {
        if (tableView                 != null                         && AssociatedObject                     != null &&
            e.RowData.RowHandle.Value == GridControl.NewItemRowHandle && AssociatedObject.SelectedItems.Count == 0)
        {
            NewItemCommand?.Execute(parameter: null);
            tableView.FocusedRowHandle = -1;
        }
    }

}
