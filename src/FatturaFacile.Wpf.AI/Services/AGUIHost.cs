// <copyright file="AGUIHost.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI.Agents;
using FatturaFacile.Wpf.AI.Agents.Weather;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.AI.Services;

/// <summary>
/// In-process ASP.NET Core web host providing AG-UI protocol endpoints for local AI agents.
/// </summary>
public sealed class AGUIHost : IAsyncDisposable, IDisposable
{

    /// <summary>
    /// The hosted web application instance.
    /// </summary>
    private WebApplication app;

    /// <summary>
    /// The base URL where the local host is running.
    /// </summary>
    private string baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AGUIHost"/> class.
    /// </summary>
    private AGUIHost()
    {
    }

    /// <summary>
    /// Gets the base loopback URL on which the local ASP.NET Core server is actively listening.
    /// </summary>
    public string BaseUrl => baseUrl;

    /// <summary>
    /// Starts an in-process ASP.NET Core web host binding a dynamic local port and registering AG-UI server endpoints.
    /// </summary>
    /// <param name="agentFactory">The factory responsible for creating configured agent instances.</param>
    /// <param name="weatherTools">Weather tool providers bound into the agent execution context.</param>
    /// <returns>A started <see cref="AGUIHost"/> instance.</returns>
    public static async Task<AGUIHost> StartAsync(
        AgentFactory      agentFactory,
        WeatherAgentTools weatherTools)
    {
        var host = new AGUIHost();

        var builder = WebApplication.CreateBuilder();
        _ = builder.Services.AddAGUIServer();

        _ = builder.AddAIAgent(
                               name: "WeatherAgent",
                               createAgentDelegate: (sp, key) =>
                                                    {
                                                        return agentFactory.CreateWeatherAgent(weatherTool: weatherTools.GetWeatherTool(), name: key);
                                                    });

        _ = builder.WebHost.UseUrls(urls: "http://127.0.0.1:0");

        host.app = builder.Build();

        _ = host.app.MapAGUIServer(
                                   pattern: "/api/weather-agent",
                                   aiAgent: host.app.Services.GetRequiredKeyedService<AIAgent>(serviceKey: "WeatherAgent"));

        await host.app.StartAsync().ConfigureAwait(continueOnCapturedContext: false);

        var server           = host.app.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        host.baseUrl = addressesFeature?.Addresses.FirstOrDefault()?.TrimEnd(trimChar: '/') ?? "http://localhost:5099";

        return host;
    }

    /// <summary>
    /// Gets the full URL endpoint route exposing the Weather Agent via the AG-UI protocol.
    /// </summary>
    /// <returns>The complete URL string for the weather agent endpoint.</returns>
    public string GetWeatherAgentEndpoint() => $"{baseUrl}/api/weather-agent";

    /// <summary>
    /// Indicates whether this host has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        (app as IDisposable)?.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return default;
        }

        if (app != null)
        {
            return new ValueTask(task: DisposeAsyncCore());
        }

        _disposed = true;

        return default;
    }

    /// <summary>
    /// Performs asynchronous disposal of the underlying web application.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    private async Task DisposeAsyncCore()
    {
        try
        {
            await app.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
        }
        finally
        {
            _disposed = true;
        }
    }

}
