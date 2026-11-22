// <copyright file="InkSignature.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using FatturaFacile.Application.FieldService;

namespace FatturaFacile.Wpf.Behaviors;

/// <summary>
/// Makes an <see cref="InkCanvas"/> a signature pad a view model can bind to without code-behind: the drawn strokes are
/// published as a <see cref="FieldServiceInkSketch"/> (pure data, converted to SVG elsewhere), and two counters let the view
/// model clear the pad or take back the last stroke.
/// </summary>
[SuppressMessage("Design", "MA0182:Avoid unused internal types", Justification = "Used from XAML.")]
internal static class InkSignature
{
    /// <summary>
    /// Identifies the IsEnabled attached property.
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled",
        typeof(bool),
        typeof(InkSignature),
        new PropertyMetadata(defaultValue: false, OnIsEnabledChanged));

    /// <summary>
    /// Identifies the Sketch attached property (bind it one way to source).
    /// </summary>
    public static readonly DependencyProperty SketchProperty = DependencyProperty.RegisterAttached(
        "Sketch",
        typeof(FieldServiceInkSketch),
        typeof(InkSignature),
        new FrameworkPropertyMetadata(defaultValue: null, FrameworkPropertyMetadataOptions.None));

    /// <summary>
    /// Identifies the ClearRequest attached property.
    /// </summary>
    public static readonly DependencyProperty ClearRequestProperty = DependencyProperty.RegisterAttached(
        "ClearRequest",
        typeof(int),
        typeof(InkSignature),
        new PropertyMetadata(defaultValue: 0, OnClearRequested));

    /// <summary>
    /// Identifies the UndoRequest attached property.
    /// </summary>
    public static readonly DependencyProperty UndoRequestProperty = DependencyProperty.RegisterAttached(
        "UndoRequest",
        typeof(int),
        typeof(InkSignature),
        new PropertyMetadata(defaultValue: 0, OnUndoRequested));

    /// <summary>
    /// Gets a value indicating whether the canvas publishes its strokes.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <returns>The flag.</returns>
    public static bool GetIsEnabled(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (bool)target.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// Sets a value indicating whether the canvas publishes its strokes.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <param name="value">The flag.</param>
    public static void SetIsEnabled(DependencyObject target, bool value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Gets the published sketch.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <returns>The sketch, or null before the first stroke.</returns>
    public static FieldServiceInkSketch? GetSketch(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (FieldServiceInkSketch?)target.GetValue(SketchProperty);
    }

    /// <summary>
    /// Sets the published sketch.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <param name="value">The sketch.</param>
    public static void SetSketch(DependencyObject target, FieldServiceInkSketch? value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(SketchProperty, value);
    }

    /// <summary>
    /// Gets the clear request counter.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <returns>The counter.</returns>
    public static int GetClearRequest(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (int)target.GetValue(ClearRequestProperty);
    }

    /// <summary>
    /// Sets the clear request counter: every change clears the pad.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <param name="value">The counter.</param>
    public static void SetClearRequest(DependencyObject target, int value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(ClearRequestProperty, value);
    }

    /// <summary>
    /// Gets the undo request counter.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <returns>The counter.</returns>
    public static int GetUndoRequest(DependencyObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return (int)target.GetValue(UndoRequestProperty);
    }

    /// <summary>
    /// Sets the undo request counter: every change takes back the last stroke.
    /// </summary>
    /// <param name="target">The canvas.</param>
    /// <param name="value">The counter.</param>
    public static void SetUndoRequest(DependencyObject target, int value)
    {
        ArgumentNullException.ThrowIfNull(target);
        target.SetValue(UndoRequestProperty, value);
    }

    private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not InkCanvas canvas)
        {
            return;
        }

        canvas.StrokeCollected -= OnStrokesChanged;
        canvas.StrokeErased -= OnStrokesErased;
        canvas.SizeChanged -= OnSizeChanged;

        if (e.NewValue is true)
        {
            canvas.StrokeCollected += OnStrokesChanged;
            canvas.StrokeErased += OnStrokesErased;
            canvas.SizeChanged += OnSizeChanged;
        }
    }

    private static void OnClearRequested(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not InkCanvas canvas)
        {
            return;
        }

        canvas.Strokes.Clear();
        Publish(canvas);
    }

    private static void OnUndoRequested(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not InkCanvas canvas || canvas.Strokes.Count == 0)
        {
            return;
        }

        canvas.Strokes.RemoveAt(canvas.Strokes.Count - 1);
        Publish(canvas);
    }

    private static void OnStrokesChanged(object sender, InkCanvasStrokeCollectedEventArgs e) => Publish(sender);

    private static void OnStrokesErased(object sender, RoutedEventArgs e) => Publish(sender);

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is InkCanvas { Strokes.Count: > 0 } canvas)
        {
            Publish(canvas);
        }
    }

    private static void Publish(object sender)
    {
        if (sender is not InkCanvas canvas)
        {
            return;
        }

        List<IReadOnlyList<FieldServiceInkPoint>> strokes = [];
        foreach (var stroke in canvas.Strokes)
        {
            strokes.Add(stroke.StylusPoints.Select(point => new FieldServiceInkPoint(point.X, point.Y)).ToList());
        }

        canvas.SetCurrentValue(SketchProperty, new FieldServiceInkSketch(strokes, canvas.ActualWidth, canvas.ActualHeight));
    }
}
