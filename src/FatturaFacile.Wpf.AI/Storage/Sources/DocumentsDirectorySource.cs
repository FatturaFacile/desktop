// <copyright file="DocumentsDirectorySource.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text;

using DevExpress.AIIntegration.Extensions.Chunks;
using DevExpress.Pdf;
using DevExpress.Spreadsheet;
using DevExpress.Xpf.RichEdit;
using FatturaFacile.Wpf.AI.Storage.Models;

namespace FatturaFacile.Wpf.AI.Storage.Sources;

/// <summary>
/// Loads and extracts vectorized documents and chunks from a local file directory.
/// </summary>
public class DocumentDirectorySource(string sourceDirectory) : IDocumentsSource
{

    /// <summary>
    /// Represents an extracted text chunk with page and positional coordinates.
    /// </summary>
    /// <param name="Page">The 1-based page index.</param>
    /// <param name="IndexOnPage">The index of the chunk within the page.</param>
    /// <param name="Text">The chunk text content.</param>
    private record ContentChunk(int Page, int IndexOnPage, string Text);

    /// <summary>
    /// Extracts the canonical document identifier (file name) from a disk file path.
    /// </summary>
    /// <param name="path">The full or relative file system path.</param>
    /// <returns>The extracted filename identifier.</returns>
    public static string SourceFileId(string path) => Path.GetFileName(path: path);

    /// <summary>
    /// Produces a version stamp based on the last modified UTC timestamp of the file.
    /// </summary>
    /// <param name="path">The target file system path.</param>
    /// <returns>An ISO 8601 formatted timestamp string.</returns>
    public static string SourceFileVersion(string path) => File.GetLastWriteTimeUtc(path: path).ToString(format: "o");

    /// <summary>
    /// File extensions recognized for text extraction and vector indexing.
    /// </summary>
    private static readonly string[] SupportedExtensions =
    {
        ".pdf", ".docx", ".rtf", ".html", ".htm", ".txt", ".xls", ".xlsx",
    };

    /// <inheritdoc />
    public Task<IEnumerable<VectorizedDocument>> GetDocumentsToVectorizeAsync()
    {
        var files = Directory.GetFiles(path: sourceDirectory)
                             .Where(predicate: f => SupportedExtensions.Contains(value: Path.GetExtension(path: f).ToLowerInvariant()));

        IEnumerable<VectorizedDocument> results = files.Select(selector: file => new VectorizedDocument
                                                                                 {
                                                                                     Key             = Guid.NewGuid().ToString(),
                                                                                     DocumentId      = SourceFileId(path: file),
                                                                                     DocumentVersion = SourceFileVersion(path: file),
                                                                                 })
                                                       .ToList();

        return Task.FromResult(result: results);
    }

    /// <inheritdoc />
    public Task<IEnumerable<VectorizedDocumentChunk>> GetChunksToVectorizeAsync(VectorizedDocument document)
    {
        var filePath  = Path.Combine(path1: sourceDirectory, path2: document.DocumentId);
        var extension = Path.GetExtension(path: filePath).ToLowerInvariant();
        var chunks    = new List<ContentChunk>();

        if (extension == ".pdf")
        {
            chunks.AddRange(collection: GetPdfContent(filePath: filePath));
        }
        else if (extension == ".xls" || extension == ".xlsx")
        {
            chunks.AddRange(collection: GetExcelContent(filePath: filePath));
        }
        else if (extension is ".docx" or ".rtf" or ".doc" or ".html" or ".htm" or ".txt")
        {
            chunks.AddRange(collection: GetWordContent(filePath: filePath));
        }
        else
        {
            throw new NotSupportedException(message: $"File format '{extension}' is not supported.");
        }

        return Task.FromResult(
                               result: chunks.Select(selector: p => new VectorizedDocumentChunk
                                                                    {
                                                                        Key        = Guid.NewGuid().ToString(),
                                                                        DocumentId = document.DocumentId,
                                                                        Page       = p.Page,
                                                                        Content    = p.Text,
                                                                    }));
    }

    /// <summary>
    /// Extracts text chunks from PDF documents page by page.
    /// </summary>
    /// <param name="filePath">The file system path to the PDF.</param>
    /// <returns>A collection of content chunks.</returns>
    private static IEnumerable<ContentChunk> GetPdfContent(string filePath)
    {
        var       chunks    = new List<ContentChunk>();
        using var processor = new PdfDocumentProcessor();
        processor.LoadDocument(path: filePath);

        for (var pageIndex = 1; pageIndex <= processor.Document.Pages.Count; pageIndex++)
        {
            var pageText = processor.GetPageText(
                                                 pageNumber: pageIndex,
                                                 options: new PdfTextExtractionOptions
                                                          {
                                                              ClipToCropBox = false,
                                                          });

            if (string.IsNullOrWhiteSpace(value: pageText))
            {
                continue;
            }

            chunks.AddRange(collection: GetChunks(pageNumber: pageIndex, pageText: pageText));
        }

        return chunks;
    }

    /// <summary>
    /// Extracts text chunks from Word, RTF, HTML, and text documents page by page.
    /// </summary>
    /// <param name="filePath">The file system path to the document.</param>
    /// <returns>A list of content chunks.</returns>
    private static List<ContentChunk> GetWordContent(string filePath)
    {
        List<ContentChunk> chunks = new();

        System.Windows.Application.Current.Dispatcher.Invoke(callback: () =>
                                                                       {
                                                                           var richEdit = new RichEditControl();
                                                                           _ = richEdit.LoadDocument(fileName: filePath);

                                                                           var pageCount = richEdit.DocumentLayout.GetPageCount();

                                                                           for (var i = 0; i < pageCount; i++)
                                                                           {
                                                                               var page = richEdit.DocumentLayout.GetPage(pageIndex: i);

                                                                               var range = richEdit.Document.CreateRange(
                                                                                                                         start: page
                                                                                                                               .MainContentRange
                                                                                                                               .Start,
                                                                                                                         length: page
                                                                                                                                .MainContentRange
                                                                                                                                .Length);

                                                                               var pageContentText = richEdit.Document
                                                                                                             .GetText(range: range)
                                                                                                             .Trim(trimChars: ['\r', '\n']);

                                                                               if (!string.IsNullOrWhiteSpace(value: pageContentText))
                                                                               {
                                                                                   chunks.AddRange(
                                                                                                   collection: GetChunks(
                                                                                                                         pageNumber: i + 1,
                                                                                                                         pageText: pageContentText));
                                                                               }
                                                                           }
                                                                       });

        return chunks;
    }

    /// <summary>
    /// Extracts text chunks from Excel spreadsheets worksheet by worksheet.
    /// </summary>
    /// <param name="filePath">The file system path to the spreadsheet.</param>
    /// <returns>A list of content chunks.</returns>
    private static List<ContentChunk> GetExcelContent(string filePath)
    {
        List<ContentChunk> chunks   = new();
        using var          workbook = new Workbook();
        _ = workbook.LoadDocument(fileName: filePath);

        for (var sheetIndex = 0; sheetIndex < workbook.Worksheets.Count; sheetIndex++)
        {
            var sheet = workbook.Worksheets[index: sheetIndex];
            var used  = sheet.GetUsedRange();

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
                    var val = sheet.Cells[rowOffset: r, columnOffset: c].DisplayText;

                    if (string.IsNullOrWhiteSpace(value: val))
                    {
                        continue;
                    }

                    cellValues.Add(item: val.Trim());
                }

                if (cellValues.Count > 0)
                {
                    _ = sheetSb.AppendLine(value: string.Join(separator: '\t', values: cellValues));
                }
            }

            var pageText = sheetSb.ToString();

            if (!string.IsNullOrWhiteSpace(value: pageText))
            {
                chunks.AddRange(collection: GetChunks(pageNumber: sheetIndex + 1, pageText: pageText));
            }
        }

        return chunks;
    }

    /// <summary>
    /// Splits page text into paragraph chunks of up to 1000 characters.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageText">The full text of the page.</param>
    /// <returns>A collection of generated content chunks.</returns>
    private static IEnumerable<ContentChunk> GetChunks(
        int    pageNumber,
        string pageText)
    {
        return ParagraphTextToChunkConverter.Instance.SplitToChunks(userInput: pageText, maximumSymbolsCount: 1000)
                                            .Select(selector: (chunk, index) => new ContentChunk(
                                                                                                 Page: pageNumber,
                                                                                                 IndexOnPage: index,
                                                                                                 Text: pageText.Substring(
                                                                                                                          startIndex: chunk
                                                                                                                             .StartIndex,
                                                                                                                          length: chunk
                                                                                                                             .Length)));
    }

}
