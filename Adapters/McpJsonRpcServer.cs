using System.Text.Json;
using System.Text.Json.Nodes;
using DocumentMcpServer.Core.Interfaces;

namespace DocumentMcpServer.Adapters;

/// <summary>
/// MCP JSON-RPC protocol adapter for DocumentService.
/// Handles protocol-level concerns, delegates business logic to IDocumentService.
/// </summary>
public class McpJsonRpcServer(IDocumentService documentService)
{
    private readonly IDocumentService _documentService = documentService;

    /// <summary>
    /// Processes a JSON-RPC request and returns a JSON-RPC response.
    /// </summary>
    public string HandleRequest(string jsonRequest)
    {
        try
        {
            var request = JsonSerializer.Deserialize<JsonNode>(jsonRequest);
            if (request == null)
            {
                return CreateErrorResponse(null, "Invalid JSON");
            }

            var method = request["method"]?.GetValue<string>();
            var id = request["id"];
            var params_ = request["params"];

            object result = method switch
            {
                McpMethod.Initialize => HandleInitialize(),
                McpMethod.ToolsList => HandleToolsList(),
                McpMethod.ToolsCall => HandleToolsCall(params_),
                _ => new ToolCallResponse
                {
                    Content =
                    [
                        new ContentItem { Type = "text", Text = $"Unknown method: {method}" }
                    ]
                }
            };

            return CreateSuccessResponse(id, result);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(null, ex.Message);
        }
    }

    private static InitializeResponse HandleInitialize()
    {
        return new InitializeResponse
        {
            ProtocolVersion = "2025-06-18",
            Capabilities = new ServerCapabilities
            {
                Tools = new ToolsCapability()
            },
            ServerInfo = new ServerInfo
            {
                Name = "document-server",
                Version = "1.0.0"
            }
        };
    }

    private static ToolsListResponse HandleToolsList()
    {
        var tools = new ToolDefinition[]
        {
            new()
            {
                Name = McpTool.SearchDocuments,
                Description = "Search through documents for specific keywords or phrases. Returns matching documents with relevant excerpts and file paths.",
                InputSchema = new InputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertySchema>
                    {
                        ["query"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "The search query (e.g., 'termination clauses', 'payment terms')"
                        }
                    },
                    Required = ["query"]
                }
            },
            new()
            {
                Name = McpTool.GetDocument,
                Description = "Retrieve the full content of a specific document by its ID. Returns the document with its file path.",
                InputSchema = new InputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertySchema>
                    {
                        ["documentId"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "The unique identifier of the document"
                        }
                    },
                    Required = ["documentId"]
                }
            },
            new()
            {
                Name = McpTool.ListDocuments,
                Description = "List all available documents with their metadata (ID, title, date, file path).",
                InputSchema = new InputSchema
                {
                    Type = "object",
                    Properties = [],
                    Required = []
                }
            },
            new()
            {
                Name = McpTool.SummarizeDocument,
                Description = "Get a summary of a specific document including key points and file path.",
                InputSchema = new InputSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, PropertySchema>
                    {
                        ["documentId"] = new PropertySchema
                        {
                            Type = "string",
                            Description = "The unique identifier of the document to summarize"
                        }
                    },
                    Required = ["documentId"]
                }
            }
        };

        return new ToolsListResponse { Tools = tools };
    }

    private ToolCallResponse HandleToolsCall(JsonNode? params_)
    {
        if (params_ == null)
        {
            return CreateToolResponse("No parameters provided");
        }

        var toolName = params_["name"]?.GetValue<string>() ?? "";
        var arguments = params_["arguments"]?.AsObject();

        return toolName switch
        {
            McpTool.SearchDocuments => HandleSearchDocuments(arguments),
            McpTool.GetDocument => HandleGetDocument(arguments),
            McpTool.ListDocuments => HandleListDocuments(),
            McpTool.SummarizeDocument => HandleSummarizeDocument(arguments),
            _ => CreateToolResponse($"Unknown tool: {toolName}")
        };
    }

    private ToolCallResponse HandleSearchDocuments(JsonObject? args)
    {
        var query = args?["query"]?.GetValue<string>() ?? "";
        var results = _documentService.SearchDocuments(query);
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        return CreateToolResponse(json);
    }

    private ToolCallResponse HandleGetDocument(JsonObject? args)
    {
        var documentId = args?["documentId"]?.GetValue<string>() ?? "";
        var document = _documentService.GetDocument(documentId);

        if (document == null)
        {
            return CreateToolResponse($"Document '{documentId}' not found.");
        }

        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        return CreateToolResponse(json);
    }

    private ToolCallResponse HandleListDocuments()
    {
        var documents = _documentService.ListDocuments();
        var json = JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
        return CreateToolResponse(json);
    }

    private ToolCallResponse HandleSummarizeDocument(JsonObject? args)
    {
        var documentId = args?["documentId"]?.GetValue<string>() ?? "";
        var summary = _documentService.SummarizeDocument(documentId);
        return CreateToolResponse(summary);
    }

    private static ToolCallResponse CreateToolResponse(string text)
    {
        return new ToolCallResponse
        {
            Content =
            [
                new ContentItem { Type = "text", Text = text }
            ]
        };
    }

    private static string CreateSuccessResponse(JsonNode? id, object result)
    {
        object? responseId = null;
        if (id != null)
        {
            responseId = id.GetValueKind() == JsonValueKind.Number
                ? id.GetValue<int>()
                : (object?)id.GetValue<string>();
        }

        var response = new JsonRpcResponse
        {
            Jsonrpc = "2.0",
            Id = responseId,
            Result = result
        };

        return JsonSerializer.Serialize(response);
    }

    private static string CreateErrorResponse(JsonNode? id, string errorMessage)
    {
        object? responseId = null;
        if (id != null)
        {
            responseId = id.GetValueKind() == JsonValueKind.Number
                ? id.GetValue<int>()
                : (object?)id.GetValue<string>();
        }

        var response = new JsonRpcErrorResponse
        {
            Jsonrpc = "2.0",
            Id = responseId,
            Error = new ErrorInfo { Message = errorMessage }
        };

        return JsonSerializer.Serialize(response);
    }
}
