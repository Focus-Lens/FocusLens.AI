using System.Text.Json;
using FocusLens.AI.Models;

namespace FocusLens.AI.Services;

public class AnswerEvaluationService
{
    private readonly GeminiService _geminiService;

    public AnswerEvaluationService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<AnswerEvaluationResult> EvaluateAsync(
        AnswerEvaluationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("Question is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SelectedAnswer))
        {
            throw new ArgumentException("Selected answer is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CorrectAnswer))
        {
            throw new ArgumentException("Correct answer is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required.");
        }

        var prompt = $"""
        Evaluate the student's answer for FocusLens.

        Use ONLY the provided educational content and question.

        Determine:
        1. Whether the student's answer is correct.
        2. The learning signal for the related concept.

        The learning signal must describe the learning state
        indicated by this answer.

        Use ONLY one of these learning signals:
        - understood
        - needs_review

        Return ONLY valid JSON.

        The JSON must contain:
        isCorrect
        sectionId
        conceptId
        learningSignal

        Do not add markdown code fences.
        Do not add explanations outside the JSON.

        Question:
        {request.Question}

        Student answer:
        {request.SelectedAnswer}

        Correct answer:
        {request.CorrectAnswer}

        Section ID:
        {request.SectionId}

        Concept ID:
        {request.ConceptId}

        Educational content:
        {request.Content}
        """;

        var response = await _geminiService.GenerateAsync(prompt);

        return ParseGeminiResponse(response);
    }

    private static AnswerEvaluationResult ParseGeminiResponse(
        string response)
    {
        using var document = JsonDocument.Parse(response);

        var root = document.RootElement;

        if (!root.TryGetProperty("steps", out var steps))
        {
            throw new InvalidOperationException(
                "Gemini response does not contain a steps field.");
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

                var result =
                    JsonSerializer.Deserialize<AnswerEvaluationResult>(
                        jsonText,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (result == null)
                {
                    throw new InvalidOperationException(
                        "Failed to deserialize answer evaluation result.");
                }

                if (string.IsNullOrWhiteSpace(result.SectionId) ||
                    string.IsNullOrWhiteSpace(result.ConceptId) ||
                    string.IsNullOrWhiteSpace(result.LearningSignal))
                {
                    throw new InvalidOperationException(
                        "Answer evaluation result is incomplete.");
                }

                return result;
            }
        }

        throw new InvalidOperationException(
            "No model output text was found in Gemini response.");
    }
}