using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace D3sk.AgentCli.Output.Common;

/// <summary>Reading helpers every stdout parser shares.</summary>
public static class Json
{
    private static readonly JsonSerializerOptions Readable = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary><paramref name="value"/> when it is a JSON object, else an empty one.</summary>
    public static JsonObject AsObject(JsonNode? value) => value as JsonObject ?? [];

    /// <summary>
    /// The CLI's whole tool result as text when the parser kept less of it; "" when it kept it all.
    /// InjectionGuard scans this too (<c>ToolResult.Raw</c>): a field the parser does not show is
    /// still text the model read. A non-string value is its compact JSON, non-ASCII kept as is.
    /// </summary>
    public static string RawText(JsonNode? value, string kept)
    {
        if (value is null)
        {
            return "";
        }

        var raw = value is JsonValue v && v.TryGetValue<string>(out var s) ? s : value.ToJsonString(Readable);
        return raw == kept ? "" : raw;
    }

    /// <summary>The string value of <paramref name="value"/>, or null when it is not a JSON string.</summary>
    public static string? AsString(JsonNode? value) =>
        value is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

    /// <summary>Python truthiness of a JSON value: null, false, 0, "", [] and {} are not.</summary>
    public static bool IsTruthy(JsonNode? value) => value switch
    {
        null => false,
        JsonArray a => a.Count > 0,
        JsonObject o => o.Count > 0,
        JsonValue v => v.GetValueKind() switch
        {
            JsonValueKind.False or JsonValueKind.Null => false,
            JsonValueKind.String => v.GetValue<string>().Length > 0,
            JsonValueKind.Number => v.GetValue<JsonElement>().GetDouble() != 0,
            _ => true,
        },
        _ => true,
    };
}
