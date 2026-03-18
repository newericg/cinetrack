using System.Text.Json;

namespace CineTrack.API.Services;

/// <summary>
/// Fetches anime poster URLs from Jikan API (MyAnimeList).
/// No API key required. Rate limit: ~3 requests/second.
/// </summary>
public class JikanService
{
    private readonly HttpClient _http;
    private const string BasePath = "/v4/anime";

    public JikanService(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("Jikan");
        _http.DefaultRequestHeaders.Add("User-Agent", "CineTrack/1.0");
    }

    /// <summary>
    /// Gets the poster URL for an anime by MAL ID. Returns null on failure.
    /// </summary>
    public async Task<string?> GetPosterUrlAsync(int malId, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"{BasePath}/{malId}", ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("data", out var data)) return null;
            if (!data.TryGetProperty("images", out var images)) return null;
            if (!images.TryGetProperty("jpg", out var jpg)) return null;

            // Prefer large_image_url for poster quality
            return jpg.TryGetProperty("large_image_url", out var large)
                ? large.GetString()
                : jpg.TryGetProperty("image_url", out var img) ? img.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
