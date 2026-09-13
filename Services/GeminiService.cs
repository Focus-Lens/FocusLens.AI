using System.Text;
using System.Text.Json;

namespace FocusLens.AI.Services;

public class GeminiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public GeminiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> GenerateAsync(string prompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key was not found.");
        }

        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Remove("x-goog-api-key");
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        var requestBody = new
        {
            model = "gemini-3.8-flash",
            input = prompt
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(
            "https://generativelanguage.googleapis.com/v1beta/interactions",
            content
        );

        var responseText =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API error ({(int)response.StatusCode}): {responseText}");
        }

        return responseText;
    }


    public async Task<string> GenerateImageContentAsync(
        byte[] imageBytes,
        string mimeType,
        string prompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key was not found.");
        }

        if (imageBytes == null || imageBytes.Length == 0)
        {
            throw new ArgumentException(
                "Image data is required.");
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new ArgumentException(
                "Image MIME type is required.");
        }

        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Remove("x-goog-api-key");
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        var base64Image = Convert.ToBase64String(imageBytes);

        var requestBody = new
        {
            model = "gemini-3.8-flash",
            input = new
            {
                text = prompt,
                image = new
                {
                    mimeType = mimeType,
                    data = base64Image
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync(
            "https://generativelanguage.googleapis.com/v1beta/interactions",
            content
        );

        var responseText =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini image API error ({(int)response.StatusCode}): {responseText}");
        }

        return responseText;
    }
}