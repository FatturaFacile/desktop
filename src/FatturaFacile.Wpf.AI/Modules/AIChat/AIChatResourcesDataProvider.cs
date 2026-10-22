// <copyright file="AIChatResourcesDataProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;

using DevExpress.Data.Utils;
using DevExpress.Security;
using DevExpress.AIIntegration.Blazor.Chat;
using Microsoft.Extensions.AI;

namespace FatturaFacile.Modules.AIChat;

/// <summary>
/// Provides cached domain attachments and reference files (logs, screenshots, markdown documentation) to the AI chat workspace.
/// </summary>
static class AIChatResourcesDataProvider
{
    private static readonly Dictionary<string, AIChatResource> resources;

    static AIChatResourcesDataProvider()
    {
        var accessLog = new AIChatResource(
            "access.txt", "Access Log", GetLogResource, "text/plain", "Web server access log for the past 24 hours.");
        var dashboardImage = new AIChatResource(
            "dashboard.jpg", "Dashboard (Screenshot)", GetImageResource, "image/jpeg",
            "Latest dashboard screenshot for real-time data analysis.");
        var chatDocumentation = new AIChatResource(
            "dxaichat.md", "AI Chat Documentation", GetDocsResource, "text/markdown",
            "Reference guide for AI Chat functionality and usage.");

        resources = new Dictionary<string, AIChatResource>
        {
            { accessLog.Uri, accessLog },
            { dashboardImage.Uri, dashboardImage },
            { chatDocumentation.Uri, chatDocumentation }
        };
    }

    private static string GetFilePath(
        string fileName)
    {
        AIChatResource resource;
        if (resources.TryGetValue(fileName, out resource))
        {
            var root = SafeEnvironment.CurrentDirectory;
            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("Cannot find the current directory.");
            }

            var path = Path.Combine(root, "Resources", "AIChat", resource.Uri);
            if (!SafePath.IsRooted(root, path))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Resource '{0}' path '{1}' is outside the allowed root directory '{2}'.", fileName, path,
                        root));
            }

            return path;
        }

        throw new InvalidOperationException(string.Format("Resource '{0}' not found.", fileName));
    }

    private static async Task<IList<AIContent>> GetLogResource(
        AIChatResource resource,
        CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(GetFilePath(resource.Uri), cancellationToken);
        return new List<AIContent> { new TextContent(text) };
    }

    private static async Task<ReadOnlyMemory<byte>> GetImageResource(
        AIChatResource resource,
        CancellationToken cancellationToken)
    {
#pragma warning disable DX0025
        return await File.ReadAllBytesAsync(GetFilePath(resource.Uri), cancellationToken);
#pragma warning restore DX0025
    }

    private static async Task<string> GetDocsResource(
        AIChatResource resource,
        CancellationToken cancellationToken)
    {
#pragma warning disable DX0025
        return await File.ReadAllTextAsync(GetFilePath(resource.Uri), cancellationToken);
#pragma warning restore DX0025
    }

    /// <summary>
    /// Retrieves the list of predefined sample resources available for querying and referencing in the AI chat demo.
    /// </summary>
    /// <returns>A collection of registered <see cref="AIChatResource"/> items.</returns>
    public static List<AIChatResource> GetData() => resources.Values.ToList();
}
