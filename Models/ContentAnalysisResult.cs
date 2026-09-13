using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class ContentAnalysisResult
{
    [JsonPropertyName("sections")]
    public List<ContentSection> Sections { get; set; } = new();
}

public class ContentSection
{
    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("concepts")]
    public List<ContentConcept> Concepts { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("estimatedTimeMinutes")]
    public int EstimatedTimeMinutes { get; set; }
}

public class ContentConcept
{
    [JsonPropertyName("conceptId")]
    public string ConceptId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("keyLearningPoints")]
    public List<string> KeyLearningPoints { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("estimatedTimeMinutes")]
    public int EstimatedTimeMinutes { get; set; }
}