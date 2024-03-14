using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Library.Clients;

public class ScryfallApiClient
{
    private readonly string _baseUrl = "https://api.scryfall.com";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScryfallApiClient> _logger;

    public ScryfallApiClient(ILogger<ScryfallApiClient> logger)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _logger = logger;
    }

    public async Task DownloadCards(Dictionary<string, int> cards, string? outputPath = null)
    {
        try
        {
            outputPath = CreateFolderForPictures(outputPath);

            foreach (var card in cards)
            {
                var cardName = card.Key;
                var cardQuantity = card.Value;
                var images = await GetCardImageUrlsFromScryfall(cardName);
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        await DownloadImage(image.Value, outputPath, image.Key, cardQuantity);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }


    private async Task<Dictionary<string, string>?> GetCardImageUrlsFromScryfall(string cardName)
    {
        var requestUrl = $"/cards/search?q=${cardName}";
        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(json);
            Dictionary<string, string> imagesUrl = new();
            if (jsonObject?["data"] is null || jsonObject?["data"]?.Count() == 0)
            {
                return null;
            }

            var index = 0;
            for (int i = 0; i < jsonObject?["data"]!.Count(); i++)
            {
                var foundCard = jsonObject?["data"]!;
                var foundCardName = foundCard[i]?["name"]?.ToString() ?? string.Empty;
                if (foundCardName.Equals(cardName, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            // Handle two dual side cards as well
            if (jsonObject?["data"]?[index]?["card_faces"] is not null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var sideName = jsonObject?["data"]?[index]?["card_faces"]?[i]?["name"]?.Value<string>()!;
                    var imageUrl = jsonObject?["data"]?[index]?["card_faces"]?[i]?["image_uris"]?["large"]?.Value<string>()!;
                    imagesUrl.Add(sideName, imageUrl);
                }
            }
            // Single page card
            if (imagesUrl.Count == 0 || imagesUrl.Any(i => i.Value is null))
            {
                imagesUrl.Clear();
                imagesUrl.Add(cardName, jsonObject?["data"]?[index]?["image_uris"]?["large"]?.Value<string>()!);
            }

            return imagesUrl;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }
    }

    private async Task DownloadImage(string imageUrl, string folderPath, string filename, int quantity)
    {
        using var client = new HttpClient();
        try
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

            var filePath = Path.Combine(folderPath, $"{quantity}_{filename.Replace(" // ", "-")}.jpg");
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch (Exception)
        {
        }
    }

    private string CreateFolderForPictures(string? directoryName)
    {
        if (directoryName == null)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "ArchidektPrinter_output");
        }

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return Path.GetFullPath(directoryName);
    }
}
