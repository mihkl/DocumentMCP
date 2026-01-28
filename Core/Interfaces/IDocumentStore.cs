using DocumentMcpServer.Core.Models;

namespace DocumentMcpServer.Core.Interfaces;

public interface IDocumentStore
{
    IEnumerable<Document> LoadDocuments();
    Document? GetById(string id);
}
