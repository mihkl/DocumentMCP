using DocumentMcpServer.Core.Interfaces;
using DocumentMcpServer.Core.Services;
using DocumentMcpServer.Infrastructure.Extractors;
using DocumentMcpServer.Infrastructure.Storage;
using DocumentMcpServer.Adapters.JsonRpc;
using DocumentMcpServer.Adapters.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var mode = Environment.GetEnvironmentVariable("MCP_MODE") ?? "stdio";
var documentsPath = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "Documents");

if (mode.Equals("http", StringComparison.OrdinalIgnoreCase))
{
    var builder = WebApplication.CreateBuilder(args);

    RegisterCoreServices(builder.Services, documentsPath);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapDocumentEndpoints();

    Console.WriteLine("Document Service HTTP API starting...");
    Console.WriteLine($"Documents path: {documentsPath}");
    Console.WriteLine("Swagger UI: http://localhost:5000/swagger");

    app.Run("http://localhost:5000");
}
else
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
        builder.SetMinimumLevel(LogLevel.Information);
    });

    Console.Error.WriteLine("Document MCP Server starting...");
    Console.Error.WriteLine($"Documents path: {documentsPath}");

    var extractors = CreateDefaultExtractors();

    var storeLogger = loggerFactory.CreateLogger<FileSystemDocumentStore>();
    var store = new FileSystemDocumentStore(documentsPath, extractors, storeLogger);

    IDocumentIndex searchIndex = new DocumentIndex();

    var serviceLogger = loggerFactory.CreateLogger<DocumentService>();
    IDocumentService documentService = new DocumentService(store, searchIndex, serviceLogger);

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
            
            // Only output non-empty responses (requests get responses, notifications don't)
            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine(response);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }
}

static IDocumentExtractor[] CreateDefaultExtractors() =>
[
    new TextFileExtractor(),
    new DocxExtractor(),
    new XlsxExtractor(),
    new PdfExtractor()
];

static void RegisterCoreServices(IServiceCollection services, string documentsPath)
{
    var extractors = CreateDefaultExtractors();
    
    services.AddSingleton<IEnumerable<IDocumentExtractor>>(extractors);
    services.AddSingleton<IDocumentStore>(sp => 
        new FileSystemDocumentStore(
            documentsPath, 
            sp.GetRequiredService<IEnumerable<IDocumentExtractor>>(),
            sp.GetRequiredService<ILogger<FileSystemDocumentStore>>()));
    services.AddSingleton<IDocumentIndex, DocumentIndex>();
    services.AddSingleton<IDocumentService>(sp =>
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var index = sp.GetRequiredService<IDocumentIndex>();
        var logger = sp.GetRequiredService<ILogger<DocumentService>>();
        return new DocumentService(store, index, logger);
    });
}
