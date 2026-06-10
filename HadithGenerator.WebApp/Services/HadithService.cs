using HadithGenerator.Models.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HadithGenerator.Services;

public class HadithService : IHadithService
{
    private const int PageLimit = 10;
    private const int MaxAttempts = 10;
    private const string SahihStatusBN = "সহিহ";
    private const string DefaultBaseUrl =
        "https://www.hadithbd.com/search/hadith/search_query.php?q=%E0%A6%AE%E0%A6%BF%E0%A6%B6%E0%A6%95%E0%A6%BE%E0%A6%A4%E0%A7%81%E0%A6%B2%20%E0%A6%AE%E0%A6%BE%E0%A6%B8%E0%A6%BE%E0%A6%AC%E0%A7%80%E0%A6%B9%20(%E0%A6%AE%E0%A6%BF%E0%A6%B6%E0%A6%95%E0%A6%BE%E0%A6%A4)&limit=10&page=1";

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public HadithService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = string.IsNullOrWhiteSpace(configuration["HadithApi:BaseUrl"])
            ? DefaultBaseUrl
            : configuration["HadithApi:BaseUrl"]!;

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 HadithGenerator/1.0");
        }
    }

    public async Task<HadithViewModel> GenerateNew()
    {
        return await GetRandomHadith();
    }

    public async Task<HadithViewModel> GetRandomHadith()
    {
        var totalPages = await GetTotalPages();

        if (totalPages == 0)
        {
            return new HadithViewModel();
        }

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            HadithSearchResponse? response;
            var page = Random.Shared.Next(1, totalPages + 1);
            var requestUrl = BuildPagedUrl(page);

            try
            {
                response = await _httpClient.GetFromJsonAsync<HadithSearchResponse>(requestUrl);
            }
            catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
            {
                return new HadithViewModel();
            }

            var sahihHits = (response?.Hits ?? [])
                .Where(hit => hit.IsSahih)
                .ToList();

            if (sahihHits.Count == 0)
            {
                continue;
            }

            var randomHadith = sahihHits[Random.Shared.Next(sahihHits.Count)];
            return randomHadith.ToViewModel();
        }

        return new HadithViewModel();
    }

    private async Task<int> GetTotalPages()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<HadithSearchResponse>(BuildPagedUrl(1));
            return response?.EstimatedTotalHits > 0
                ? (int)Math.Ceiling(response.EstimatedTotalHits / (double)PageLimit)
                : 0;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or NotSupportedException)
        {
            return 0;
        }
    }

    private string BuildPagedUrl(int page)
    {
        return SetQueryParameter(SetQueryParameter(_baseUrl, "limit", PageLimit.ToString()), "page", page.ToString());
    }

    private static string SetQueryParameter(string url, string key, string value)
    {
        var uriBuilder = new UriBuilder(url);
        var queryParts = uriBuilder.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        var existingIndex = queryParts.FindIndex(part =>
        {
            var separatorIndex = part.IndexOf('=');
            var partKey = separatorIndex >= 0 ? part[..separatorIndex] : part;
            return string.Equals(partKey, key, StringComparison.OrdinalIgnoreCase);
        });

        var queryPart = $"{key}={value}";
        if (existingIndex >= 0)
        {
            queryParts[existingIndex] = queryPart;
        }
        else
        {
            queryParts.Add(queryPart);
        }

        uriBuilder.Query = string.Join('&', queryParts);
        return uriBuilder.ToString();
    }

    private sealed record HadithSearchResponse(
        [property: JsonPropertyName("hits")] List<HadithSearchHit> Hits,
        [property: JsonPropertyName("estimatedTotalHits")] int EstimatedTotalHits);

    private sealed record HadithSearchHit
    {
        [JsonPropertyName("hadith_no")]
        public int HadithNo { get; init; }

        [JsonPropertyName("bangla_raw")]
        public string? BanglaRaw { get; init; }

        [JsonPropertyName("note_raw")]
        public string? NoteRaw { get; init; }

        [JsonPropertyName("book_name_bn")]
        public string? BookNameBN { get; init; }

        [JsonPropertyName("section_bn")]
        public string? SectionBN { get; init; }

        [JsonPropertyName("status_bn")]
        public string? StatusBN { get; init; }

        [JsonPropertyName("explanation_raw")]
        public string? ExplanationRaw { get; init; }

        public bool IsSahih =>
            StatusBN?.Trim().StartsWith(SahihStatusBN, StringComparison.Ordinal) == true;

        public HadithViewModel ToViewModel()
        {
            return new HadithViewModel
            {
                HadithNo = HadithNo,
                BanglaRaw = BanglaRaw ?? string.Empty,
                NoteRaw = NoteRaw ?? string.Empty,
                BookNameBN = BookNameBN ?? string.Empty,
                SectionBN = SectionBN ?? string.Empty,
                StatusBN = StatusBN ?? string.Empty,
                ExplanationRaw = ExplanationRaw ?? string.Empty
            };
        }
    }
}
