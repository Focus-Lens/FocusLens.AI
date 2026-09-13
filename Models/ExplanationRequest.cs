using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class ExplanationRequest
{
    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("conceptId")]
    public string ConceptId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}