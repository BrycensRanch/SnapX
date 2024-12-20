using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Path;

namespace SnapX.Core.Utils.Extensions;

public static class JsonExtensions
{
    public static JsonNode SelectToken(this JsonNode node, string path,
        PathEvaluationOptions options = null)
    {
        var jsonPath = JsonPath.Parse(path);
        var matches = jsonPath.Evaluate(node, options).Matches;
        if (matches.Count > 1)
            throw new JsonException("Path returned multiple tokens.");
        return matches.FirstOrDefault()?.Value;
    }

    public static List<JsonNode> SelectTokens(this JsonNode node, string path,
        PathEvaluationOptions options = null)
    {
        var jsonPath = JsonPath.Parse(path);
        return
            jsonPath.Evaluate(node, options)
                .Matches
                .Select(m => m.Value)
                .ToList();
    }

    public static IList<JsonNode> Children(this JsonNode node)
    {
        return node switch
        {
            JsonArray array => array.ToList(),
            JsonObject obj => obj.Select(e => e.Value).ToList(),
            _ => []
        };
    }

    public static JsonNode? ParseJsonNode(string text)
    {
        // Handle empty or null input text
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }

}
