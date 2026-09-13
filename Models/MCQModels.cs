using System.Text.Json.Serialization;

namespace FocusLens.AI.Models;

public class MCQGenerationRequest
{
    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("conceptId")]
    public string ConceptId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("numberOfQuestions")]
    public int NumberOfQuestions { get; set; } = 3;
}

public class MCQGenerationResult
{
    [JsonPropertyName("questions")]
    public List<MCQQuestion> Questions { get; set; } = new();
}

public class MCQQuestion
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("sectionId")]
    public string SectionId { get; set; } = string.Empty;

    [JsonPropertyName("conceptId")]
    public string ConceptId { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
}