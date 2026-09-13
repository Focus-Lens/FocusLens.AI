using System.Text.Json;
using FocusLens.AI.Models;

namespace FocusLens.AI.Services;

public class MCQGenerationService
{
    private readonly GeminiService _geminiService;

    public MCQGenerationService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<MCQGenerationResult> GenerateAsync(
        MCQGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SectionId))
        {
            throw new ArgumentException("SectionId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ConceptId))
        {
            throw new ArgumentException("ConceptId is required.");
        }

        var numberOfQuestions = Math.Clamp(
            request.NumberOfQuestions,
            1,
            10);

        var prompt = $"""
        Generate multiple-choice questions for FocusLens.

        Use ONLY the educational content provided below.

        Requirements:

        1. Generate exactly {numberOfQuestions} questions.
        2. Each question must test understanding or retrieval.
        3. Each question must have exactly 4 options.
        4. Only one option must be correct.
        5. Include the correct answer.
        6. Include a concise explanation of why the correct answer is correct.
        7. Do not use information outside the provided content.
        8. Difficulty must be one of: easy, medium, hard.
        9. Use the provided sectionId for every question.
        10. Use the provided conceptId for every question.
        11. Do not duplicate questions.

        Return ONLY valid JSON.

        The root object must contain:
        questions

        Each question object must contain:
        question
        options
        correctAnswer
        sectionId
        conceptId
        difficulty
        explanation

        Do not add markdown code fences.
        Do not add explanations outside the JSON.

        Use this sectionId:
        {request.SectionId}

        Use this conceptId:
        {request.ConceptId}

        Educational content:

        {request.Content}
        """;

        var response = await _geminiService.GenerateAsync(prompt);

        return ParseGeminiResponse(response);
    }

    private static MCQGenerationResult ParseGeminiResponse(
        string response)
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

                return JsonSerializer.Deserialize<MCQGenerationResult>(
                    jsonText,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                    ?? throw new InvalidOperationException(
                        "Failed to deserialize MCQ generation result.");
            }
        }

        throw new InvalidOperationException(
            "No model output text was found in Gemini response.");
    }
}