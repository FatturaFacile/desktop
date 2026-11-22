// <copyright file="ReceivablesProvisionRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Domain.Receivables;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A bucket of the bad-debt provision with its editable rate.
/// </summary>
internal sealed partial class ReceivablesProvisionRow : ObservableObject
{
    private readonly Action<ReceivablesProvisionRow> _changed;

    [ObservableProperty]
    private string _rateText;

    [ObservableProperty]
    private string _provisionText;

    [ObservableProperty]
    private bool _isDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesProvisionRow"/> class.
    /// </summary>
    /// <param name="bucket">The bucket.</param>
    /// <param name="label">The bucket label.</param>
    /// <param name="openAmount">The open amount in the bucket.</param>
    /// <param name="openText">The open amount as text.</param>
    /// <param name="rateText">The rate as text.</param>
    /// <param name="provisionText">The provision as text.</param>
    /// <param name="isDefault">Whether the rate is the default one.</param>
    /// <param name="changed">Called when the user edits the rate.</param>
    public ReceivablesProvisionRow(AgingBucket bucket, string label, decimal openAmount, string openText, string rateText, string provisionText, bool isDefault, Action<ReceivablesProvisionRow> changed)
    {
        ArgumentNullException.ThrowIfNull(bucket);
        ArgumentNullException.ThrowIfNull(changed);

        Bucket = bucket;
        Label = label;
        OpenAmount = openAmount;
        OpenText = openText;
        _rateText = rateText;
        _provisionText = provisionText;
        _isDefault = isDefault;
        _changed = changed;
    }

    /// <summary>
    /// Gets the bucket.
    /// </summary>
    public AgingBucket Bucket { get; }

    /// <summary>
    /// Gets the bucket label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the open amount in the bucket.
    /// </summary>
    public decimal OpenAmount { get; }

    /// <summary>
    /// Gets the open amount as text.
    /// </summary>
    public string OpenText { get; }

    partial void OnRateTextChanged(string value) => _changed(this);
}
