using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Core.Interfaces;

public interface IDocumentService
{
    List<DocumentSummary> ListDocuments();
    Document? GetDocument(string documentId);
    List<SearchResult> SearchDocuments(string query);
    string SummarizeDocument(string documentId);
    event Action<string>? Log;
}
