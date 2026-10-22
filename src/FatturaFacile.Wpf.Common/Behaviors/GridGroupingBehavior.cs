// <copyright file="GridGroupingBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;

namespace FatturaFacile.Behaviors;

/// <summary>
/// Attached behavior for DevExpress <see cref="GridControl"/> that coordinates dynamic column grouping and new item command execution.
/// </summary>
public class GridGroupingBehavior : Behavior<GridControl>
{
    private TableView? tableView;

    /// <summary>
    /// Identifies the <see cref="Group"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GroupProperty =
        DependencyProperty.Register(
            "Group",
            typeof(string),
            typeof(GridGroupingBehavior),
            new FrameworkPropertyMetadata(null, (d, e) => ((GridGroupingBehavior)d).OnGroupChanged(e)));

    /// <summary>
    /// Gets or sets the field name by which the grid rows are grouped.
    /// When <see langword="null"/> or empty, existing grouping is cleared.
    /// </summary>
    public string? Group
    {
        get => (string?)GetValue(GroupProperty);
        set => SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="NewItemCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty NewItemCommandProperty =
        DependencyProperty.Register(
            "NewItemCommand",
            typeof(ICommand),
            typeof(GridGroupingBehavior),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the command executed when the new item row is focused.
    /// </summary>
    public ICommand? NewItemCommand
    {
        get => (ICommand?)GetValue(NewItemCommandProperty);
        set => SetValue(NewItemCommandProperty, value);
    }

    private void OnGroupChanged(
        DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(Group))
        {
            AssociatedObject.ClearGrouping();
        }

        if (Group != null && AssociatedObject.Columns.GetColumnByFieldName(Group) != null)
        {
            AssociatedObject.GroupBy(Group);
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

    private void Loaded(
        object sender,
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

    private void FocusedRowHandleChanged(
        object sender,
        FocusedRowHandleChangedEventArgs e)
    {
        if (tableView != null && AssociatedObject != null &&
            e.RowData.RowHandle.Value == GridControl.NewItemRowHandle && AssociatedObject.SelectedItems.Count == 0)
        {
            NewItemCommand?.Execute(null);
            tableView.FocusedRowHandle = -1;
        }
    }
}
