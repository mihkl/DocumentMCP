namespace DocumentMcpServer.Adapters.JsonRpc;

/// <summary>
/// MCP protocol method names.
/// </summary>
public static class McpMethod
{
    public const string Initialize = "initialize";
    public const string ToolsList = "tools/list";
    public const string ToolsCall = "tools/call";
}

/// <summary>
/// Registered MCP tool names.
/// </summary>
public static class McpTool
{
    public const string SearchDocuments = "search_documents";
    public const string GetDocument = "get_document";
    public const string ListDocuments = "list_documents";
    public const string SummarizeDocument = "summarize_document";
}

/// <summary>
/// Tool definition for MCP protocol.
/// </summary>
public class ToolDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required InputSchema InputSchema { get; init; }
}

/// <summary>
/// JSON Schema for tool input parameters.
/// </summary>
public class InputSchema
{
    public required string Type { get; init; }
    public required Dictionary<string, PropertySchema> Properties { get; init; }
    public required string[] Required { get; init; }
}

/// <summary>
/// JSON Schema property definition.
/// </summary>
public class PropertySchema
{
    public required string Type { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// MCP initialize response.
/// </summary>
public class InitializeResponse
{
    public required string ProtocolVersion { get; init; }
    public required ServerCapabilities Capabilities { get; init; }
    public required ServerInfo ServerInfo { get; init; }
}

/// <summary>
/// Server capabilities.
/// </summary>
public class ServerCapabilities
{
    public required ToolsCapability Tools { get; init; }
}

/// <summary>
/// Tools capability (empty object for now).
/// </summary>
public class ToolsCapability
{
}

/// <summary>
/// Server information.
/// </summary>
public class ServerInfo
{
    public required string Name { get; init; }
    public required string Version { get; init; }
}

/// <summary>
/// Tools list response.
/// </summary>
public class ToolsListResponse
{
    public required ToolDefinition[] Tools { get; init; }
}

/// <summary>
/// Tool call response (MCP content format).
/// </summary>
public class ToolCallResponse
{
    public required ContentItem[] Content { get; init; }
}

/// <summary>
/// Content item (text-based for now).
/// </summary>
public class ContentItem
{
    public required string Type { get; init; }
    public required string Text { get; init; }
}

/// <summary>
/// JSON-RPC success response.
/// </summary>
public class JsonRpcResponse
{
    public required string Jsonrpc { get; init; }
    public object? Id { get; init; }
    public object? Result { get; init; }
}

/// <summary>
/// JSON-RPC error response.
/// </summary>
public class JsonRpcErrorResponse
{
    public required string Jsonrpc { get; init; }
    public object? Id { get; init; }
    public required ErrorInfo Error { get; init; }
}

/// <summary>
/// Error information.
/// </summary>
public class ErrorInfo
{
    public required string Message { get; init; }
}
