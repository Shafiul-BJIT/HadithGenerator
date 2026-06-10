using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using HadithGenerator.Models.ViewModels;

namespace HadithGenerator.Services;

public class OpenRouterHadithImageService : IHadithImageService
{
    private const string DefaultModel = "sourceful/riverflow-v2.5-pro";
    private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";

    private static readonly (string Name, string Direction)[] Designs =
    [
        (
            "Emerald Geometry",
            "Emerald, ivory, and subtle gold Islamic geometry. Calm, refined, high contrast."
        ),
        (
            "Midnight Mosque",
            "Deep navy night, restrained mosque silhouette, moonlight, and subtle gold accents."
        ),
        (
            "Paper & Arch",
            "Warm handmade paper, minimal Islamic arch, sage, charcoal, and editorial typography."
        )
    ];

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _designCount;

    public OpenRouterHadithImageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenRouterImage:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? string.Empty;
        _model = configuration["OpenRouterImage:Model"] ?? DefaultModel;
        _designCount = Math.Clamp(
            configuration.GetValue("OpenRouterImage:DesignCount", 1),
            1,
            Designs.Length);
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);
    public int DesignCount => _designCount;

    public async Task<IReadOnlyList<HadithImageDesignViewModel>> GenerateDesigns(
        HadithViewModel hadith,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return [];
        }

        var hadithText = ExtractHadithText(hadith.BanglaRaw);
        var tasks = Designs.Take(_designCount).Select(design =>
            GenerateDesign(design, hadith, hadithText, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results
            .Where(result => result is not null)
            .Cast<HadithImageDesignViewModel>()
            .ToList();
    }

    private async Task<HadithImageDesignViewModel?> GenerateDesign(
        (string Name, string Direction) design,
        HadithViewModel hadith,
        string hadithText,
        CancellationToken cancellationToken)
    {
        var prompt = $"""
            Create a polished 16:9 Bengali Islamic hadith banner.
            Design direction: {design.Direction}
            Use tasteful Islamic visual motifs. No people, faces, animals, logos, or watermarks.
            Render the following Bengali text exactly as provided. Do not translate, rewrite,
            summarize, correct, omit, or invent any words.

            Book: {hadith.BookNameBN}
            Hadith number: {hadith.HadithNo}
            Section: {hadith.SectionBN}
            Status: {hadith.StatusBN}

            Hadith:
            {hadithText}

            Keep all text readable with generous padding and strong contrast.
            """;

        var request = new
        {
            model = _model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            modalities = new[] { "image" },
            image_config = new
            {
                aspect_ratio = "16:9"
            }
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        message.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<OpenRouterImageResponse>(
            stream,
            cancellationToken: cancellationToken);
        var images = result?.Choices?
            .FirstOrDefault()?
            .Message?
            .Images;

        if (images is null)
        {
            return null;
        }

        foreach (var image in images)
        {
            if (!TryParseDataUrl(image.ImageUrl?.Url, out var mimeType, out var base64Data))
            {
                continue;
            }

            return new HadithImageDesignViewModel
            {
                Name = design.Name,
                MimeType = mimeType,
                Base64Data = base64Data
            };
        }

        return null;
    }

    private static string ExtractHadithText(string html)
    {
        var content = html;
        var horizontalRuleIndex = html.IndexOf("<hr", StringComparison.OrdinalIgnoreCase);

        if (horizontalRuleIndex >= 0)
        {
            var tagEndIndex = html.IndexOf('>', horizontalRuleIndex);
            if (tagEndIndex >= 0)
            {
                content = html[(tagEndIndex + 1)..];
            }
        }

        var plainText = Regex.Replace(content, "<[^>]+>", " ");
        plainText = WebUtility.HtmlDecode(plainText);
        return Regex.Replace(plainText, @"\s+", " ").Trim();
    }

    private static bool TryParseDataUrl(
        string? dataUrl,
        out string mimeType,
        out string base64Data)
    {
        mimeType = "image/png";
        base64Data = string.Empty;

        if (string.IsNullOrWhiteSpace(dataUrl)
            || !dataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var commaIndex = dataUrl.IndexOf(',');
        if (commaIndex < 0)
        {
            return false;
        }

        var metadata = dataUrl[5..commaIndex];
        var separatorIndex = metadata.IndexOf(';');
        if (separatorIndex > 0)
        {
            mimeType = metadata[..separatorIndex];
        }

        base64Data = dataUrl[(commaIndex + 1)..];
        return metadata.Contains(";base64", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(base64Data);
    }

    private sealed record OpenRouterImageResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenRouterChoice>? Choices { get; init; }
    }

    private sealed record OpenRouterChoice
    {
        [JsonPropertyName("message")]
        public OpenRouterMessage? Message { get; init; }
    }

    private sealed record OpenRouterMessage
    {
        [JsonPropertyName("images")]
        public List<OpenRouterImage>? Images { get; init; }
    }

    private sealed record OpenRouterImage
    {
        [JsonPropertyName("type")]
        public string? Type { get; init; }

        [JsonPropertyName("image_url")]
        public OpenRouterImageUrl? ImageUrl { get; init; }
    }

    private sealed record OpenRouterImageUrl
    {
        [JsonPropertyName("url")]
        public string? Url { get; init; }
    }
}
