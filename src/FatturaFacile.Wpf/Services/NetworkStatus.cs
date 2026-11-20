// <copyright file="NetworkStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// <see cref="INetworkStatus"/> backed by the operating system network availability events.
/// </summary>
internal sealed class NetworkStatus : INetworkStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkStatus"/> class.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3366:\"this\" should not be exposed from constructors", Justification = "Singleton created by the DI container; the event only forwards to Changed.")]
    public NetworkStatus()
    {
        NetworkChange.NetworkAvailabilityChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public bool IsOnline => NetworkInterface.GetIsNetworkAvailable();
}
