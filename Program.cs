using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Services;
using DocumentMcpServer.Infrastructure.Extractors;
using DocumentMcpServer.Infrastructure.Storage;
using DocumentMcpServer.Adapters;
using Microsoft.Extensions.Logging;

var documentsPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "Documents");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    builder.SetMinimumLevel(LogLevel.Information);
});

Console.Error.WriteLine("Document MCP Server starting...");
Console.Error.WriteLine($"Documents path: {documentsPath}");

IDocumentExtractor[] extractors =
[
    new TextFileExtractor(),
    new DocxExtractor(),
    new XlsxExtractor(),
    new PdfExtractor()
];

var storeLogger = loggerFactory.CreateLogger<FileSystemDocumentStore>();
var store = new FileSystemDocumentStore(documentsPath, extractors, storeLogger);

var serviceLogger = loggerFactory.CreateLogger<DocumentService>();
IDocumentService documentService = new DocumentService(store, serviceLogger);

var mcpServer = new McpJsonRpcServer(documentService);

Console.Error.WriteLine("Available tools: search_documents, get_document, list_documents, summarize_document");
Console.Error.WriteLine("Listening on stdio...");

while (true)
{
    var line = Console.ReadLine();
    if (line == null) break;

    try
    {
        var response = mcpServer.HandleRequest(line);
        Console.WriteLine(response);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
    }
}
