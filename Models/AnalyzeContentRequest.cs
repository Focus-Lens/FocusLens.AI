using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class AnalyzeContentRequest
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("fromPage")]
    public int? FromPage { get; set; }

    [JsonPropertyName("toPage")]
    public int? ToPage { get; set; }
}