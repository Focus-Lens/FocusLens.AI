using System.Text.Json;
using FocusLens.AI.Models;

namespace FocusLens.AI.Services;

public class ContentAnalysisService
{
    private readonly GeminiService _geminiService;

    public ContentAnalysisService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<ContentAnalysisResult> AnalyzeAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.");
        }

        var prompt = $"""
        Analyze the following educational content for FocusLens.

        Your task is to convert the provided educational content into a structured
        learning representation.

        Identify:
        1. Main sections and their titles.
        2. Key concepts within each section.
        3. Key learning points for each concept.
        4. Difficulty of each section and concept: easy, medium, or hard.
        5. Estimated study time in minutes for each concept and section.
        6. The first and last page containing each section.

        Generate a unique sectionId for every section.
        Generate a unique conceptId for every concept.

        Requirements:
        - Preserve the meaning of the original educational content.
        - Do not invent concepts or facts that are not supported by the content.
        - Keep sections logically organized.
        - Keep concepts specific and meaningful for learning.
        - Key learning points must represent information actually present in the content.
        - Difficulty must be one of: easy, medium, hard.
        - estimatedTimeMinutes must be a positive integer.
        - sectionId values must be unique.
        - conceptId values must be unique.
        - Every concept must belong to exactly one section.
        - fromPage and toPage must refer to the [Page N] markers in the supplied content.
        - Page numbers are relative to this supplied PDF, beginning at 1.
        - fromPage must be at least 1 and toPage must be greater than or equal to fromPage.

        Return ONLY valid JSON.
        Do not add markdown code fences.
        Do not add explanations outside the JSON.

        The JSON must contain:
        sections

        Each section must contain:
        sectionId
        title
        difficulty
        estimatedTimeMinutes
        fromPage
        toPage
        concepts

        Each concept must contain:
        conceptId
        name
        keyLearningPoints
        difficulty
        estimatedTimeMinutes

        Educational content:
        {content}
        """;

        var response = await _geminiService.GenerateAsync(prompt);

        return ParseGeminiResponse(response);
    }

    private static ContentAnalysisResult ParseGeminiResponse(string response)
    {
        using var document = JsonDocument.Parse(response);

        var root = document.RootElement;

        if (!root.TryGetProperty("steps", out var steps))
        {
            throw new InvalidOperationException(
                "Gemini response does not contain a steps field.");
        }

        if (steps.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "Gemini steps is not an array.");
        }

        foreach (var step in steps.EnumerateArray())
        {
            if (!step.TryGetProperty("type", out var type))
                continue;

            if (type.GetString() != "model_output")
                continue;

            if (!step.TryGetProperty("content", out var content))
                continue;

            if (content.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var contentItem in content.EnumerateArray())
            {
                if (!contentItem.TryGetProperty("text", out var text))
                    continue;

                var jsonText = text.GetString();

                if (string.IsNullOrWhiteSpace(jsonText))
                    continue;

                var result = JsonSerializer.Deserialize<ContentAnalysisResult>(
                    jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null)
                {
                    throw new InvalidOperationException(
                        "Failed to deserialize content analysis result.");
                }

                ValidatePageRanges(result);
                return result;
            }
        }

        throw new InvalidOperationException(
            "No model output text was found in Gemini response.");
    }

    private static void ValidatePageRanges(ContentAnalysisResult result)
    {
        foreach (ContentSection section in result.Sections)
        {
            if (section.FromPage < 1 || section.ToPage < section.FromPage)
            {
                throw new InvalidOperationException(
                    $"AI section '{section.Title}' has an invalid page range.");
            }
        }
    }
}
