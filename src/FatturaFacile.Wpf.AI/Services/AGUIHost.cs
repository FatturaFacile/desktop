// <copyright file="AGUIHost.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Agents;
using FatturaFacile.Agents.Weather;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Services;

/// <summary>
/// In-process ASP.NET Core web host providing AG-UI protocol endpoints for local AI agents.
/// </summary>
public sealed class AGUIHost : IAsyncDisposable, IDisposable
{
    private WebApplication app;
    private string baseUrl;

    private AGUIHost()
    {
    }

    public string BaseUrl => baseUrl;

    public static async Task<AGUIHost> StartAsync(
        AgentFactory agentFactory,
        WeatherAgentTools weatherTools)
    {
        var host = new AGUIHost();

        var builder = WebApplication.CreateBuilder();
        _ = builder.Services.AddAGUIServer();
        _ = builder.AddAIAgent(
            "WeatherAgent",
            (sp, key) => { return agentFactory.CreateWeatherAgent(weatherTools.GetWeatherTool(), key); });
        _ = builder.WebHost.UseUrls("http://127.0.0.1:0");

        host.app = builder.Build();
        _ = host.app.MapAGUIServer(
            "/api/weather-agent",
            host.app.Services.GetRequiredKeyedService<AIAgent>("WeatherAgent"));

        await host.app.StartAsync().ConfigureAwait(false);

        var server = host.app.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        host.baseUrl = addressesFeature?.Addresses.FirstOrDefault()?.TrimEnd('/') ?? "http://localhost:5099";

        return host;
    }

    public string GetWeatherAgentEndpoint() => $"{baseUrl}/api/weather-agent";

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        (app as IDisposable)?.Dispose();
        _disposed = true;
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return default;
        }

        if (app != null)
        {
            return new ValueTask(DisposeAsyncCore());
        }

        _disposed = true;
        return default;
    }

    private async Task DisposeAsyncCore()
    {
        try
        {
            await app.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _disposed = true;
        }
    }
}
