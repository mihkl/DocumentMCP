using DocumentMcpServer.Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DocumentMcpServer.Adapters.Http;

/// <summary>
/// HTTP REST adapter for DocumentService.
/// Maps core document operations to HTTP endpoints.
/// </summary>
public static class HttpAdapter
{
    /// <summary>
    /// Configures HTTP endpoints for document operations.
    /// </summary>
    public static void MapDocumentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/documents", (IDocumentService service) =>
        {
            var documents = service.ListDocuments();
            return Results.Ok(documents);
        })
        .WithName("ListDocuments");

        app.MapGet("/api/documents/{id}", (string id, IDocumentService service) =>
        {
            var doc = service.GetDocument(id);
            return doc != null ? Results.Ok(doc) : Results.NotFound();
        })
        .WithName("GetDocument");

        app.MapGet("/api/search", (string q, IDocumentService service) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest(new { error = "Query parameter 'q' is required" });
            }

            var results = service.SearchDocuments(q);
            return Results.Ok(results);
        })
        .WithName("SearchDocuments");

        app.MapGet("/api/documents/{id}/summary", (string id, IDocumentService service) =>
        {
            var summary = service.SummarizeDocument(id);
            
            if (summary.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound(new { error = summary });
            }

            return Results.Ok(new { summary });
        })
        .WithName("SummarizeDocument");
    }
}
