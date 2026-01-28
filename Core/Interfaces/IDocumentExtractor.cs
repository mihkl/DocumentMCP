namespace DocumentMcpServer.Core.Interfaces;

public interface IDocumentExtractor
{
    bool CanExtract(string filePath);
    string ExtractContent(string filePath);
}
