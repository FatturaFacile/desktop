// <copyright file="AIChatResourcesDataProvider.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;

using DevExpress.Data.Utils;
using DevExpress.Security;
using DevExpress.AIIntegration.Blazor.Chat;

using Microsoft.Extensions.AI;

namespace FatturaFacile.Wpf.AI.Modules.AIChat;

/// <summary>
/// Provides cached domain attachments and reference files (logs, screenshots, markdown documentation) to the AI chat workspace.
/// </summary>
static class AIChatResourcesDataProvider
{

    /// <summary>
    /// The in-memory map of registered resource keys to AI chat resource descriptors.
    /// </summary>
    private static readonly Dictionary<string, AIChatResource> resources;

    static AIChatResourcesDataProvider()
    {
        var accessLog = new AIChatResource(
                                           uri: "access.txt",
                                           name: "Access Log",
                                           aiContentProvider: GetLogResource,
                                           mimeType: "text/plain",
                                           description: "Web server access log for the past 24 hours.");

        var dashboardImage = new AIChatResource(
                                                uri: "dashboard.jpg",
                                                name: "Dashboard (Screenshot)",
                                                binaryDataProvider: GetImageResource,
                                                mimeType: "image/jpeg",
                                                description: "Latest dashboard screenshot for real-time data analysis.");

        var chatDocumentation = new AIChatResource(
                                                   uri: "dxaichat.md",
                                                   name: "AI Chat Documentation",
                                                   textContentProvider: GetDocsResource,
                                                   mimeType: "text/markdown",
                                                   description: "Reference guide for AI Chat functionality and usage.");

        resources = new Dictionary<string, AIChatResource>
                    {
                        {
                            accessLog.Uri, accessLog
                        },
                        {
                            dashboardImage.Uri, dashboardImage
                        },
                        {
                            chatDocumentation.Uri, chatDocumentation
                        }
                    };
    }

    /// <summary>
    /// Resolves the safe absolute file system path for the specified resource.
    /// </summary>
    /// <param name="fileName">The resource file name.</param>
    /// <returns>The validated absolute path.</returns>
    private static string GetFilePath(string fileName)
    {
        AIChatResource resource;

        if (resources.TryGetValue(key: fileName, value: out resource))
        {
            var root = SafeEnvironment.CurrentDirectory;

            if (string.IsNullOrEmpty(value: root))
            {
                throw new InvalidOperationException(message: "Cannot find the current directory.");
            }

            var path = Path.Combine(path1: root, path2: "Resources", path3: "AIChat", path4: resource.Uri);

            return !SafePath.IsRooted(root: root, path: path)
                ? throw new InvalidOperationException(
                                                    message: string.Format(
                                                                           format: "Resource '{0}' path '{1}' is outside the allowed root directory '{2}'.",
                                                                           arg0: fileName,
                                                                           arg1: path,
                                                                           arg2: root))
                : path;
        }

        throw new InvalidOperationException(message: string.Format(format: "Resource '{0}' not found.", arg0: fileName));
    }

    /// <summary>
    /// Loads log resource contents as a list of AI text content.
    /// </summary>
    /// <param name="resource">The chat resource descriptor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task returning the list of AI content items.</returns>
    private static async Task<IList<AIContent>> GetLogResource(
        AIChatResource    resource,
        CancellationToken cancellationToken)
    {
        var text = await File.ReadAllTextAsync(path: GetFilePath(fileName: resource.Uri), cancellationToken: cancellationToken);

        return new List<AIContent>
               {
                   new TextContent(text: text)
               };
    }

    /// <summary>
    /// Reads binary data bytes for image resources.
    /// </summary>
    /// <param name="resource">The chat resource descriptor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task returning the raw byte memory.</returns>
    private static async Task<ReadOnlyMemory<byte>> GetImageResource(
        AIChatResource    resource,
        CancellationToken cancellationToken)
    {
        #pragma warning disable DX0025
        return await File.ReadAllBytesAsync(path: GetFilePath(fileName: resource.Uri), cancellationToken: cancellationToken);
        #pragma warning restore DX0025
    }

    /// <summary>
    /// Reads markdown text content for documentation resources.
    /// </summary>
    /// <param name="resource">The chat resource descriptor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation.</param>
    /// <returns>A task returning the text content.</returns>
    private static async Task<string> GetDocsResource(
        AIChatResource    resource,
        CancellationToken cancellationToken)
    {
        #pragma warning disable DX0025
        return await File.ReadAllTextAsync(path: GetFilePath(fileName: resource.Uri), cancellationToken: cancellationToken);
        #pragma warning restore DX0025
    }

    /// <summary>
    /// Retrieves the list of predefined sample resources available for querying and referencing in the AI chat demo.
    /// </summary>
    /// <returns>A collection of registered <see cref="AIChatResource"/> items.</returns>
    public static List<AIChatResource> GetData() => resources.Values.ToList();

}
