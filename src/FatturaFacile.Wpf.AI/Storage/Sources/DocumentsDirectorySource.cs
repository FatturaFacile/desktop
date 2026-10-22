// <copyright file="DocumentsDirectorySource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.AIIntegration.Extensions.Chunks;
using DevExpress.Pdf;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.RichEdit;
using FatturaFacile.Storage.Models;

namespace FatturaFacile.Storage.Sources;

/// <summary>
/// Loads and extracts vectorized documents and chunks from a local file directory.
/// </summary>
public class DocumentDirectorySource(string sourceDirectory) : IDocumentsSource
{
    private record ContentChunk(int Page, int IndexOnPage, string Text);

    public static string SourceFileId(
        string path) => Path.GetFileName(path);

    public static string SourceFileVersion(
        string path) => File.GetLastWriteTimeUtc(path).ToString("o");

    private static readonly string[] SupportedExtensions =
        { ".pdf", ".docx", ".rtf", ".html", ".htm", ".txt", ".xls", ".xlsx" };

    public Task<IEnumerable<VectorizedDocument>> GetDocumentsToVectorizeAsync()
    {
        var files = Directory.GetFiles(sourceDirectory)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        IEnumerable<VectorizedDocument> results = files.Select(file => new VectorizedDocument
        {
            Key = Guid.NewGuid().ToString(),
            DocumentId = SourceFileId(file),
            DocumentVersion = SourceFileVersion(file),
        }).ToList();
        return Task.FromResult(results);
    }

    public Task<IEnumerable<VectorizedDocumentChunk>> GetChunksToVectorizeAsync(
        VectorizedDocument document)
    {
        var filePath = Path.Combine(sourceDirectory, document.DocumentId);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var chunks = new List<ContentChunk>();

        if (extension == ".pdf")
        {
            chunks.AddRange(GetPdfContent(filePath));
        }
        else if (extension == ".xls" || extension == ".xlsx")
        {
            chunks.AddRange(GetExcelContent(filePath));
        }
        else if (extension is ".docx" or ".rtf" or ".doc" or ".html" or ".htm" or ".txt")
        {
            chunks.AddRange(GetWordContent(filePath));
        }
        else
        {
            throw new NotSupportedException($"File format '{extension}' is not supported.");
        }

        return Task.FromResult(
            chunks.Select(p => new VectorizedDocumentChunk
            {
                Key = Guid.NewGuid().ToString(),
                DocumentId = document.DocumentId,
                Page = p.Page,
                Content = p.Text,
            }));
    }

    private static IEnumerable<ContentChunk> GetPdfContent(
        string filePath)
    {
        var chunks = new List<ContentChunk>();
        using var processor = new PdfDocumentProcessor();
        processor.LoadDocument(filePath);
        for (var pageIndex = 1; pageIndex <= processor.Document.Pages.Count; pageIndex++)
        {
            var pageText = processor.GetPageText(pageIndex, new PdfTextExtractionOptions { ClipToCropBox = false });
            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            chunks.AddRange(GetChunks(pageIndex, pageText));
        }

        return chunks;
    }

    private static List<ContentChunk> GetWordContent(
        string filePath)
    {
        List<ContentChunk> chunks = new();
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var richEdit = new RichEditControl();
            richEdit.LoadDocument(filePath);

            var pageCount = richEdit.DocumentLayout.GetPageCount();
            for (var i = 0; i < pageCount; i++)
            {
                var page = richEdit.DocumentLayout.GetPage(i);
                var range = richEdit.Document.CreateRange(page.MainContentRange.Start, page.MainContentRange.Length);
                var pageContentText = richEdit.Document.GetText(range).Trim('\r', '\n');
                if (!string.IsNullOrWhiteSpace(pageContentText))
                {
                    chunks.AddRange(GetChunks(i + 1, pageContentText));
                }
            }
        });
        return chunks;
    }

    private static List<ContentChunk> GetExcelContent(
        string filePath)
    {
        List<ContentChunk> chunks = new();
        using var workbook = new Workbook();
        workbook.LoadDocument(filePath);
        for (var sheetIndex = 0; sheetIndex < workbook.Worksheets.Count; sheetIndex++)
        {
            var sheet = workbook.Worksheets[sheetIndex];
            var used = sheet.GetUsedRange();
            if (used == null)
            {
                continue;
            }

            var sheetSb = new StringBuilder();
            for (var r = used.TopRowIndex; r <= used.BottomRowIndex; r++)
            {
                List<string> cellValues = new();
                for (var c = used.LeftColumnIndex; c <= used.RightColumnIndex; c++)
                {
                    var val = sheet.Cells[r, c].DisplayText;
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        continue;
                    }

                    cellValues.Add(val.Trim());
                }

                if (cellValues.Count > 0)
                {
                    sheetSb.AppendLine(string.Join('\t', cellValues));
                }
            }

            var pageText = sheetSb.ToString();
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                chunks.AddRange(GetChunks(sheetIndex + 1, pageText));
            }
        }

        return chunks;
    }

    private static IEnumerable<ContentChunk> GetChunks(
        int pageNumber,
        string pageText)
    {
        return ParagraphTextToChunkConverter.Instance.SplitToChunks(pageText, 1000)
            .Select((chunk, index) => new ContentChunk(
                pageNumber, index, pageText.Substring(chunk.StartIndex, chunk.Length)));
    }
}
