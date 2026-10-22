// <copyright file="AIChatFileAttachmentsModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;

using FatturaFacile.Modules.AIChat;

using Microsoft.Extensions.AI;

using DevExpress.AIIntegration.Blazor.Chat;
using DevExpress.AIIntegration.Blazor.Chat.Internal;
using DevExpress.AIIntegration.Blazor.Chat.WebView;
using DevExpress.Xpf.Editors;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for uploading, previewing, and prompting multimodal models with image and document attachments.
/// </summary>
public partial class AIChatFileAttachmentsModule
{

    /// <summary>
    /// File name for the sample Eiffel Tower image asset.
    /// </summary>
    private const string ImageName = "Eiffel-Tower.jpg";

    /// <summary>
    /// File name for the generic file icon SVG asset.
    /// </summary>
    private const string FileIconName = "file.svg";

    /// <summary>
    /// Mapping of attachment types to their permitted file extensions.
    /// </summary>
    private static readonly Dictionary<FileAttachmentType, List<string>> fileExtensions =
        new Dictionary<FileAttachmentType, List<string>>
        {
            {
                FileAttachmentType.Images, new List<string>
                                           {
                                               ".jpg",
                                               ".png",
                                               ".jpeg",
                                           }
            },
            {
                FileAttachmentType.Documents, new List<string>
                                              {
                                                  ".pdf",
                                                  ".docx",
                                              }
            },
            {
                FileAttachmentType.Tables, new List<string>
                                           {
                                               ".xlsx",
                                               ".csv",
                                           }
            },
        };

    /// <summary>
    /// Mapping of attachment types to their permitted MIME types.
    /// </summary>
    private static readonly Dictionary<FileAttachmentType, List<string>> mimeTypes =
        new Dictionary<FileAttachmentType, List<string>>
        {
            {
                FileAttachmentType.Images, new List<string>
                                           {
                                               "image/jpeg",
                                               "image/png",
                                           }
            },
            {
                FileAttachmentType.Documents, new List<string>
                                              {
                                                  "application/pdf",
                                                  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                              }
            },
            {
                FileAttachmentType.Tables, new List<string>
                                           {
                                               "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                               "text/csv",
                                           }
            },
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="AIChatFileAttachmentsModule"/> class.
    /// </summary>
    public AIChatFileAttachmentsModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;

        aiChatControl.MessageTemplate = message => builder =>
                                                   {
                                                       builder.OpenComponent<AIChatFileAttachmentsMessageTemplate>(sequence: 0);

                                                       builder.AddAttribute(
                                                                            sequence: 1,
                                                                            name: nameof(AIChatFileAttachmentsMessageTemplate
                                                                                            .Context),
                                                                            value: message);

                                                       builder.AddAttribute(
                                                                            sequence: 2,
                                                                            name: nameof(AIChatFileAttachmentsMessageTemplate
                                                                                            .ImageExtensions),
                                                                            value: fileExtensions[key: FileAttachmentType.Images]);

                                                       builder.AddAttribute(
                                                                            sequence: 3,
                                                                            name: nameof(AIChatFileAttachmentsMessageTemplate
                                                                                            .FileIconBase64),
                                                                            value: GetFileIconBase64());

                                                       builder.CloseComponent();
                                                   };

        UpdateFileUploadSettings();
    }

    /// <summary>
    /// Handles changes in selected file attachment type dropdown.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event arguments containing edit values.</param>
    private void OnSelectedFileTypeChanged(
        object                    sender,
        EditValueChangedEventArgs e) => UpdateFileUploadSettings();

    /// <summary>
    /// Handles completion of AI chat control initialization, loading initial sample conversation.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event arguments for chat initialization.</param>
    private void OnAIChatControlInitialized(
        object                            sender,
        AIChatControlInitializedEventArgs e)
    {
        aiChatControl.LoadMessages(
                                   messages: new[]
                                             {
                                                 new BlazorChatMessage(
                                                                       role: ChatRole.User,
                                                                       text: "Analyze the image file and summarize its content.",
                                                                       files: CreateSampleFiles()),
                                                 new BlazorChatMessage(
                                                                       role: ChatRole.Assistant,
                                                                       text: "The image features the Eiffel Tower, a renowned landmark located in Paris, France. " +
                                                                             "The tower is shown from a low angle, emphasizing its height and intricate iron lattice structure. " +
                                                                             "The sky is lit with dramatic clouds and warm hues of sunset, creating a visually striking backdrop for the iconic monument. " +
                                                                             "The overall scene conveys a sense of grandeur and beauty, " +
                                                                             "highlighting the Eiffel Tower as a symbol of romance and architectural achievement."),
                                             });
    }

    /// <summary>
    /// Configures upload constraints and MIME filters based on the currently selected file attachment type.
    /// </summary>
    private void UpdateFileUploadSettings()
    {
        var fileType = SelectedFileType;
        var settings = new DemoFileUploadSettings();
        settings.MaxFileSize  = 10000000;
        settings.MaxFileCount = 3;
        settings.AllowedFileExtensions.AddRange(collection: fileExtensions[key: fileType]);
        settings.FileTypeFilter.AddRange(collection: mimeTypes[key: fileType]);
        aiChatControl.FileUploadSettings = settings;
    }

    /// <summary>
    /// Generates the list of sample uploaded files attached to the starter prompt.
    /// </summary>
    /// <returns>A collection of upload file descriptors.</returns>
    private List<AIChatUploadFileInfo> CreateSampleFiles()
    {
        var files = new List<AIChatUploadFileInfo>();

        try
        {
            var path = GetResourcePath(fileName: ImageName);

            if (!File.Exists(path: path))
            {
                return files;
            }
            #pragma warning disable DX0025
            var fileBytes = File.ReadAllBytes(path: path);
            #pragma warning restore DX0025
            files.Add(
                      item: new AIChatUploadFileInfo(
                                                     name: ImageName,
                                                     type: "image/jpeg",
                                                     size: fileBytes.Length,
                                                     data: new ReadOnlyMemory<byte>(array: fileBytes)));
        }
        catch
        {
        }

        return files;
    }

    /// <summary>
    /// Gets the currently chosen file attachment type.
    /// </summary>
    private FileAttachmentType SelectedFileType => selectedFileTypeEdit.EditValue is FileAttachmentType
                                                       ? (FileAttachmentType)selectedFileTypeEdit.EditValue
                                                       : FileAttachmentType.Images;

    /// <summary>
    /// Loads and converts the generic file icon asset into a Base64 data URI string.
    /// </summary>
    /// <returns>The data URI string for the icon, or empty string on error.</returns>
    private static string GetFileIconBase64()
    {
        try
        {
            var path = GetResourcePath(fileName: FileIconName);

            if (!File.Exists(path: path))
            {
                return string.Empty;
            }

            #pragma warning disable DX0025
            var fileBytes = File.ReadAllBytes(path: path);

            return "data:image/svg+xml;base64," + Convert.ToBase64String(inArray: fileBytes);
            #pragma warning restore DX0025
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Resolves the absolute path for an AI chat resource asset.
    /// </summary>
    /// <param name="fileName">The file name of the asset.</param>
    /// <returns>The full path on disk.</returns>
    private static string GetResourcePath(string fileName) =>
        Path.Combine(path1: AppDomain.CurrentDomain.BaseDirectory, path2: "Resources", path3: "AIChat", path4: fileName);

}

/// <summary>
/// Types of supported file attachments for AI chat demonstrations.
/// </summary>
public enum FileAttachmentType
{

    /// <summary>Image media files such as JPEG and PNG.</summary>
    Images,

    /// <summary>Document formats such as PDF and Word DOCX.</summary>
    Documents,

    /// <summary>Tabular data formats such as Excel XLSX and CSV.</summary>
    Tables,

}

/// <summary>
/// File upload settings implementation for AI chat file attachment demonstrations.
/// </summary>
public class DemoFileUploadSettings : IFileUploadSettings
{

    /// <summary>
    /// Gets or sets maximum permitted individual file size in bytes.
    /// </summary>
    public int MaxFileSize { get; set; }

    /// <summary>
    /// Gets or sets maximum simultaneous file upload count.
    /// </summary>
    public int MaxFileCount { get; set; }

    /// <summary>
    /// Gets the list of permitted file extensions (e.g., ".jpg", ".pdf").
    /// </summary>
    public List<string> AllowedFileExtensions { get; } = new List<string>();

    /// <summary>
    /// Gets the list of acceptable MIME types for the file chooser filter.
    /// </summary>
    public List<string> FileTypeFilter { get; } = new List<string>();

}
