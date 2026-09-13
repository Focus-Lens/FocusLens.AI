using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class ExplanationResult
{
    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("conceptId")]
    public string ConceptId { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
}