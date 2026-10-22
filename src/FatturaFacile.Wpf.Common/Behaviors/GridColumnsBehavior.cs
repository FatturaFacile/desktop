// <copyright file="GridColumnsBehavior.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;

namespace FatturaFacile.Wpf.Common.Behaviors;

/// <summary>
/// Attached behavior for DevExpress <see cref="GridControl"/> that dynamically toggles column visibility based on a visibility state.
/// </summary>
public class GridColumnsBehavior : Behavior<GridControl>
{

    /// <summary>
    /// Identifies the <see cref="ColumnsVisibility"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ColumnsVisibilityProperty = DependencyProperty.Register(
                                                                                                      name: "ColumnsVisibility",
                                                                                                      propertyType: typeof(
                                                                                                          Visibility),
                                                                                                      ownerType: typeof(
                                                                                                          GridColumnsBehavior),
                                                                                                      typeMetadata: new
                                                                                                          FrameworkPropertyMetadata(
                                                                                                                                    defaultValue: Visibility
                                                                                                                                       .Visible,
                                                                                                                                    propertyChangedCallback: (
                                                                                                                                                                     d,
                                                                                                                                                                     e) =>
                                                                                                                                                                 ((GridColumnsBehavior)
                                                                                                                                                                     d)
                                                                                                                                                                .OnColumnsVisibilityChanged(
                                                                                                                                                                                            e: e)));

    /// <summary>
    /// Gets or sets the visibility state of the managed grid columns.
    /// When set to <see cref="Visibility.Collapsed"/>, columns listed in <see cref="InvisibleColumnsName"/> are hidden.
    /// </summary>
    public Visibility ColumnsVisibility
    {
        get => (Visibility)GetValue(dp: ColumnsVisibilityProperty);
        set => SetValue(dp: ColumnsVisibilityProperty, value: value);
    }

    /// <summary>
    /// Gets or sets the field names of columns that should be collapsed when <see cref="ColumnsVisibility"/> is set to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public string[]? InvisibleColumnsName { get; set; }

    /// <summary>
    /// Handles changes to the <see cref="ColumnsVisibility"/> dependency property and updates grid column visibility.
    /// </summary>
    /// <param name="e">The event data containing the old and new property values.</param>
    private void OnColumnsVisibilityChanged(DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject?.Columns == null)
        {
            return;
        }

        if (ColumnsVisibility == Visibility.Collapsed && InvisibleColumnsName != null)
        {
            foreach (var name in InvisibleColumnsName)
            {
                var columnName = name;
                var column     = AssociatedObject.Columns.SingleOrDefault(predicate: x => x.FieldName == columnName);

                if (column != null)
                {
                    column.Visible = false;
                }
            }
        }
        else
        {
            var visibleIndex = 0;

            foreach (var column in AssociatedObject.Columns)
            {
                column.Visible      = true;
                column.VisibleIndex = visibleIndex++;
            }
        }
    }

}
