using FocusLens.AI.Models;
using FocusLens.AI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<ContentInputService>();
builder.Services.AddScoped<ContentAnalysisService>();
builder.Services.AddScoped<AIContentPipelineService>();
builder.Services.AddScoped<MCQGenerationService>();
builder.Services.AddScoped<AnswerEvaluationService>();
builder.Services.AddScoped<ExplanationService>();

var app = builder.Build();

app.MapGet("/", () => "FocusLens AI is running!");

app.MapGet("/test-gemini", async (GeminiService geminiService) =>
{
    try
    {
        var result = await geminiService.GenerateAsync(
            "Reply with exactly: FocusLens AI is connected."
        );

        return Results.Content(
            result,
            "application/json",
            System.Text.Encoding.UTF8
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/analyze-content", async (
    AnalyzeContentRequest request,
    ContentAnalysisService contentAnalysisService) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return Results.BadRequest("Content is required.");
        }

        var result = await contentAnalysisService.AnalyzeAsync(
            request.Content);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/process-content", async (
    ContentInputRequest request,
    ContentInputService contentInputService) =>
{
    try
    {
        var result = await contentInputService.ProcessAsync(request);

        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/ai/process", async (
    ContentInputRequest request,
    AIContentPipelineService pipelineService) =>
{
    try
    {
        var result = await pipelineService.ProcessAsync(request);

        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/ai/process-file", async (
    IFormFile file,
    AIContentPipelineService pipelineService) =>
{
    try
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("File is required.");
        }

        var contentType = file.ContentType?.ToLowerInvariant();

        string inputType;

        if (contentType == "application/pdf")
        {
            inputType = "pdf";
        }
        else if (contentType != null &&
                 contentType.StartsWith("image/"))
        {
            inputType = "image";
        }
        else
        {
            return Results.BadRequest(
                "Unsupported file type. Only PDF and image files are supported.");
        }

        using var memoryStream = new MemoryStream();

        await file.CopyToAsync(memoryStream);

        var request = new ContentInputRequest
        {
            InputType = inputType,
            Content = Convert.ToBase64String(memoryStream.ToArray()),
            FileName = file.FileName,
            ContentType = contentType
        };

        var result = await pipelineService.ProcessAsync(request);

        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.DisableAntiforgery();

app.MapPost("/generate-mcq", async (
    MCQGenerationRequest request,
    MCQGenerationService mcqService) =>
{
    try
    {
        var result = await mcqService.GenerateAsync(request);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/evaluate-answer", async (
    AnswerEvaluationRequest request,
    AnswerEvaluationService answerEvaluationService) =>
{
    try
    {
        var result = await answerEvaluationService.EvaluateAsync(request);

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/ai/explanation", async (
    ExplanationRequest request,
    ExplanationService explanationService) =>
{
    try
    {
        var result = await explanationService.GenerateAsync(request);

        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();