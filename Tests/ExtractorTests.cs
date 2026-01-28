using NUnit.Framework;
using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Infrastructure.Extractors;

namespace DocumentMcpServer.Tests;

public class ExtractorTests
{
    [Test]
    public void TextFileExtractor_SupportsCommonTextExtensions()
    {
        var extractor = new TextFileExtractor();

        Assert.That(extractor.CanExtract("test.txt"), Is.True);
        Assert.That(extractor.CanExtract("readme.md"), Is.True);
        Assert.That(extractor.CanExtract("config.json"), Is.True);
        Assert.That(extractor.CanExtract("script.js"), Is.True);
        Assert.That(extractor.CanExtract("style.css"), Is.True);
        Assert.That(extractor.CanExtract("program.cs"), Is.True);
    }

    [Test]
    public void TextFileExtractor_RejectsUnsupportedExtensions()
    {
        var extractor = new TextFileExtractor();

        Assert.That(extractor.CanExtract("image.png"), Is.False);
        Assert.That(extractor.CanExtract("video.mp4"), Is.False);
        Assert.That(extractor.CanExtract("archive.zip"), Is.False);
    }

    [Test]
    public void DocxExtractor_OnlyAcceptsDocxFiles()
    {
        var extractor = new DocxExtractor();

        Assert.That(extractor.CanExtract("document.docx"), Is.True);
        Assert.That(extractor.CanExtract("document.doc"), Is.False);
        Assert.That(extractor.CanExtract("document.txt"), Is.False);
    }

    [Test]
    public void XlsxExtractor_OnlyAcceptsXlsxFiles()
    {
        var extractor = new XlsxExtractor();

        Assert.That(extractor.CanExtract("spreadsheet.xlsx"), Is.True);
        Assert.That(extractor.CanExtract("spreadsheet.xls"), Is.False);
        Assert.That(extractor.CanExtract("spreadsheet.csv"), Is.False);
    }

    [Test]
    public void PdfExtractor_OnlyAcceptsPdfFiles()
    {
        var extractor = new PdfExtractor();

        Assert.That(extractor.CanExtract("document.pdf"), Is.True);
        Assert.That(extractor.CanExtract("document.PDF"), Is.True);
        Assert.That(extractor.CanExtract("document.txt"), Is.False);
    }

    [Test]
    public void ExtractorRegistry_SelectsCorrectExtractor()
    {
        var extractors = new IDocumentExtractor[]
        {
            new TextFileExtractor(),
            new DocxExtractor(),
            new XlsxExtractor(),
            new PdfExtractor()
        };

        var registry = new ExtractorRegistry(extractors);

        Assert.That(registry.GetExtractor("test.txt"), Is.InstanceOf<TextFileExtractor>());
        Assert.That(registry.GetExtractor("doc.docx"), Is.InstanceOf<DocxExtractor>());
        Assert.That(registry.GetExtractor("sheet.xlsx"), Is.InstanceOf<XlsxExtractor>());
        Assert.That(registry.GetExtractor("file.pdf"), Is.InstanceOf<PdfExtractor>());
    }

    [Test]
    public void ExtractorRegistry_ReturnsNullForUnsupportedFiles()
    {
        var extractors = new IDocumentExtractor[]
        {
            new TextFileExtractor()
        };

        var registry = new ExtractorRegistry(extractors);

        Assert.That(registry.GetExtractor("image.png"), Is.Null);
    }

    [Test]
    public void ExtractorRegistry_ExtractContent_HandlesBinaryFiles()
    {
        var extractors = new IDocumentExtractor[]
        {
            new TextFileExtractor()
        };

        var registry = new ExtractorRegistry(extractors);

        var content = registry.ExtractContent("unknown.bin");

        Assert.That(content, Does.Contain("Binary file"));
        Assert.That(content, Does.Contain("unknown.bin"));
    }
}
