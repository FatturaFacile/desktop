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

namespace FatturaFacile;

public partial class AIChatFileAttachmentsModule
{
    private const string ImageName = "Eiffel-Tower.jpg";
    private const string FileIconName = "file.svg";

    private static readonly Dictionary<FileAttachmentType, List<string>> fileExtensions =
        new Dictionary<FileAttachmentType, List<string>>
        {
            { FileAttachmentType.Images, new List<string> { ".jpg", ".png", ".jpeg" } },
            { FileAttachmentType.Documents, new List<string> { ".pdf", ".docx" } },
            { FileAttachmentType.Tables, new List<string> { ".xlsx", ".csv" } }
        };

    private static readonly Dictionary<FileAttachmentType, List<string>> mimeTypes =
        new Dictionary<FileAttachmentType, List<string>>
        {
            { FileAttachmentType.Images, new List<string> { "image/jpeg", "image/png" } },
            {
                FileAttachmentType.Documents,
                new List<string>
                    { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
            },
            {
                FileAttachmentType.Tables,
                new List<string> { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/csv" }
            }
        };

    public AIChatFileAttachmentsModule()
    {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        aiChatControl.MessageTemplate = message => builder =>
        {
            builder.OpenComponent<AIChatFileAttachmentsMessageTemplate>(0);
            builder.AddAttribute(1, nameof(AIChatFileAttachmentsMessageTemplate.Context), message);
            builder.AddAttribute(
                2, nameof(AIChatFileAttachmentsMessageTemplate.ImageExtensions),
                fileExtensions[FileAttachmentType.Images]);
            builder.AddAttribute(3, nameof(AIChatFileAttachmentsMessageTemplate.FileIconBase64), GetFileIconBase64());
            builder.CloseComponent();
        };
        ;
        UpdateFileUploadSettings();
    }

    private void OnSelectedFileTypeChanged(
        object sender,
        EditValueChangedEventArgs e) => UpdateFileUploadSettings();

    private void OnAIChatControlInitialized(
        object sender,
        AIChatControlInitializedEventArgs e)
    {
        aiChatControl.LoadMessages(
            new[]
            {
                new BlazorChatMessage(
                    ChatRole.User, "Analyze the image file and summarize its content.", CreateSampleFiles()),
                new BlazorChatMessage(
                    ChatRole.Assistant,
                    "The image features the Eiffel Tower, a renowned landmark located in Paris, France. The tower is shown from a low angle, emphasizing its height and intricate iron lattice structure. The sky is lit with dramatic clouds and warm hues of sunset, creating a visually striking backdrop for the iconic monument. The overall scene conveys a sense of grandeur and beauty, highlighting the Eiffel Tower as a symbol of romance and architectural achievement.")
            });
    }

    private void UpdateFileUploadSettings()
    {
        var fileType = SelectedFileType;
        var settings = new DemoFileUploadSettings();
        settings.MaxFileSize = 10000000;
        settings.MaxFileCount = 3;
        settings.AllowedFileExtensions.AddRange(fileExtensions[fileType]);
        settings.FileTypeFilter.AddRange(mimeTypes[fileType]);
        aiChatControl.FileUploadSettings = settings;
    }

    private List<AIChatUploadFileInfo> CreateSampleFiles()
    {
        var files = new List<AIChatUploadFileInfo>();
        try
        {
            var path = GetResourcePath(ImageName);
            if (!File.Exists(path))
            {
                return files;
            }
#pragma warning disable DX0025
            var fileBytes = File.ReadAllBytes(path);
#pragma warning restore DX0025
            files.Add(
                new AIChatUploadFileInfo(
                    ImageName, "image/jpeg", fileBytes.Length, new ReadOnlyMemory<byte>(fileBytes)));
        }
        catch
        {
        }

        return files;
    }

    private FileAttachmentType SelectedFileType
    {
        get
        {
            if (selectedFileTypeEdit.EditValue is FileAttachmentType)
            {
                return (FileAttachmentType)selectedFileTypeEdit.EditValue;
            }

            return FileAttachmentType.Images;
        }
    }

    private static string GetFileIconBase64()
    {
        try
        {
            var path = GetResourcePath(FileIconName);
            if (!File.Exists(path))
            {
                return string.Empty;
            }

#pragma warning disable DX0025
            var fileBytes = File.ReadAllBytes(path);
            return "data:image/svg+xml;base64," + Convert.ToBase64String(fileBytes);
#pragma warning restore DX0025
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetResourcePath(
        string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "AIChat", fileName);
}

/// <summary>
/// Types of supported file attachments for AI chat demonstrations.
/// </summary>
public enum FileAttachmentType
{
    Images,
    Documents,
    Tables
}

/// <summary>
/// File upload settings implementation for AI chat file attachment demonstrations.
/// </summary>
public class DemoFileUploadSettings : IFileUploadSettings
{
    public int MaxFileSize { get; set; }
    public int MaxFileCount { get; set; }
    public List<string> AllowedFileExtensions { get; } = new List<string>();
    public List<string> FileTypeFilter { get; } = new List<string>();
}
