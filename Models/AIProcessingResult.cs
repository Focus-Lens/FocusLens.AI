using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class AIProcessingResult
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "success";

    [JsonPropertyName("inputType")]
    public string InputType { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public ContentAnalysisResult? Content { get; set; }
}