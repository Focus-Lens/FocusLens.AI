using System.Text.Json;
using FocusLens.AI.Models;

namespace FocusLens.AI.Services;

public class ExplanationService
{
    private readonly GeminiService _geminiService;

    public ExplanationService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<ExplanationResult> GenerateAsync(
        ExplanationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SectionId))
        {
            throw new ArgumentException("SectionId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ConceptId))
        {
            throw new ArgumentException("ConceptId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.");
        }

        var prompt = $"""
        Generate a supplementary explanation for FocusLens.

        Use ONLY the provided educational content.

        Explain ONLY the specified concept.

        Requirements:
        1. Use simple, student-friendly language.
        2. Keep the explanation brief and targeted.
        3. Clarify the concept without rewriting the whole section.
        4. Do not introduce information outside the provided content.
        5. Do not invent facts.
        6. Preserve the original meaning of the concept.
        7. Return the provided sectionId unchanged.
        8. Return the provided conceptId unchanged.

        Return ONLY valid JSON.

        The JSON must contain these three fields:
        sectionId
        conceptId
        explanation

        Do not add markdown code fences.
        Do not add explanations outside the JSON.

        SectionId:
        {request.SectionId}

        ConceptId:
        {request.ConceptId}

        Educational content:
        {request.Content}
        """;

        var response = await _geminiService.GenerateAsync(prompt);

        using var document = JsonDocument.Parse(response);

        if (!document.RootElement.TryGetProperty(
            "steps",
            out var steps))
        {
            throw new InvalidOperationException(
                "No steps found in Gemini response.");
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
                if (!contentItem.TryGetProperty(
                    "text",
                    out var text))
                    continue;

                var jsonText = text.GetString();

                if (string.IsNullOrWhiteSpace(jsonText))
                    continue;

                var result = JsonSerializer.Deserialize<ExplanationResult>(
                    jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null)
                {
                    throw new InvalidOperationException(
                        "Failed to deserialize explanation result.");
                }

                if (string.IsNullOrWhiteSpace(result.SectionId) ||
                    string.IsNullOrWhiteSpace(result.ConceptId) ||
                    string.IsNullOrWhiteSpace(result.Explanation))
                {
                    throw new InvalidOperationException(
                        "Explanation result is incomplete.");
                }

                return result;
            }
        }

        throw new InvalidOperationException(
            "No model output text was found in Gemini response.");
    }
}