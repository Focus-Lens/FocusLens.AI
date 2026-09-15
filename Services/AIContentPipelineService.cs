using FocusLens.AI.Models;

namespace FocusLens.AI.Services;

public class AIContentPipelineService
{
    private readonly ContentInputService _contentInputService;
    private readonly ContentAnalysisService _contentAnalysisService;

    public AIContentPipelineService(
        ContentInputService contentInputService,
        ContentAnalysisService contentAnalysisService)
    {
        _contentInputService = contentInputService;
        _contentAnalysisService = contentAnalysisService;
    }

    public async Task<AIProcessingResult> ProcessAsync(
        ContentInputRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var inputResult =
            await _contentInputService.ProcessAsync(request);

        if (string.IsNullOrWhiteSpace(inputResult.Text))
        {
            throw new InvalidOperationException(
                "No educational content was extracted from the input.");
        }

        var analysisResult =
            await _contentAnalysisService.AnalyzeAsync(
                inputResult.Text);

        return new AIProcessingResult
        {
            Status = "success",
            InputType = inputResult.InputType,
            Content = analysisResult,
            ExtractedText = inputResult.Text
        };
    }
}