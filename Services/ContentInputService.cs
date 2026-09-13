using System.Text;
using System.Text.Json;
using FocusLens.AI.Models;
using UglyToad.PdfPig;

namespace FocusLens.AI.Services;

public class ContentInputService
{
    private readonly GeminiService _geminiService;

    public ContentInputService(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<ContentInputResult> ProcessAsync(
        ContentInputRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.InputType))
        {
            throw new ArgumentException("InputType is required.");
        }

        var inputType = request.InputType.Trim().ToLowerInvariant();

        string extractedText;

        switch (inputType)
        {
            case "text":
                extractedText = ExtractText(request);
                break;

            case "pdf":
                extractedText = await ExtractPdfAsync(request);
                break;

            case "image":
                extractedText = await ExtractImageAsync(request);
                break;

            default:
                throw new ArgumentException(
                    "Unsupported InputType. Supported types are: text, pdf, image.");
        }

        return new ContentInputResult
        {
            InputType = inputType,
            Text = extractedText
        };
    }

    private static string ExtractText(
        ContentInputRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Content is required for text input.");
        }

        return request.Content.Trim();
    }

    private static async Task<string> ExtractPdfAsync(
        ContentInputRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Content is required for PDF input.");
        }

        byte[] pdfBytes;

        try
        {
            pdfBytes = Convert.FromBase64String(request.Content);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "PDF Content must be a valid Base64 string.");
        }

        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfDocument.Open(stream);

        var text = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        var extractedText = text.ToString().Trim();

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidOperationException(
                "No text could be extracted from the PDF.");
        }

        return await Task.FromResult(extractedText);
    }

    private async Task<string> ExtractImageAsync(
        ContentInputRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Content is required for image input.");
        }

        if (string.IsNullOrWhiteSpace(request.ContentType))
        {
            throw new ArgumentException(
                "ContentType is required for image input.");
        }

        byte[] imageBytes;

        try
        {
            imageBytes = Convert.FromBase64String(request.Content);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "Image Content must be a valid Base64 string.");
        }

        var prompt = """
        Extract all educational text from the provided image.

        Requirements:
        - Preserve the original meaning.
        - Preserve the logical order of the content.
        - Do not summarize.
        - Do not explain.
        - Do not invent or add information.
        - Return ONLY valid JSON.

        The JSON must contain exactly:
        text

        The value of text must contain the extracted educational text.
        Do not add markdown code fences.
        Do not add any text outside the JSON.
        """;

        var response = await _geminiService.GenerateImageContentAsync(
            imageBytes,
            request.ContentType,
            prompt);

        return ParseImageOcrResponse(response);
    }

    private static string ParseImageOcrResponse(
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
                if (!contentItem.TryGetProperty(
                    "text",
                    out var text))
                    continue;

                var jsonText = text.GetString();

                if (string.IsNullOrWhiteSpace(jsonText))
                    continue;

                using var ocrDocument =
                    JsonDocument.Parse(jsonText);

                var ocrRoot = ocrDocument.RootElement;

                if (!ocrRoot.TryGetProperty(
                    "text",
                    out var extractedText))
                {
                    throw new InvalidOperationException(
                        "OCR response does not contain a text field.");
                }

                var result = extractedText.GetString();

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException(
                        "OCR returned empty text.");
                }

                return result.Trim();
            }
        }

        throw new InvalidOperationException(
            "No model output text was found in Gemini response.");
    }
}