using Library.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Library.Clients;

public class ScryfallApiClient
{
    private readonly string _baseUrl = "https://api.scryfall.com";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScryfallApiClient> _logger;
    private readonly HttpClient _imageDownloadClient;

    public ScryfallApiClient(ILogger<ScryfallApiClient> logger)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _imageDownloadClient = new HttpClient();
        _logger = logger;
    }


    public async Task UpdateCardImageLinks(Dictionary<string, MagicCardEntry> cards)
    {
        try
        {
            foreach (var card in cards)
            {
                var cardData = card.Value;
                var images = await GetCardImageUrlsFromScryfall(cardData.Name);
                if (images != null)
                {
                    cardData.ImageUrls = images;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in downloading cards from the Scryfall");
        }
    }

    public async Task<byte[]?> GetImage(string imageUrl)
    {
        try
        {
            return await _imageDownloadClient.GetByteArrayAsync(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading image from the Scryfall");
            return null;
        }
    }

    public async Task DownloadCards(Dictionary<string, MagicCardEntry> cards, string outputPath)
    {
        try
        {
            foreach (var card in cards)
            {
                var cardData = card.Value;
                var images = await GetCardImageUrlsFromScryfall(cardData.Name);
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        await DownloadImage(image.Value, outputPath, cardData.Name, cardData.Quantity);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in downloading cards from the Scryfall");
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
            _logger.LogWarning("CardName: {cardName} Card not found", cardName);
            return null;
        }
        else
        {
            _logger.LogError("CardName: {cardName} Request failed Request: {statusCode} {reasonPhrase}", cardName, response.StatusCode, response.ReasonPhrase);
            return null;
        }
    }

    private async Task DownloadImage(string imageUrl, string folderPath, string filename, int quantity)
    {
        try
        {
            byte[] imageBytes = await _imageDownloadClient.GetByteArrayAsync(imageUrl);

            var filePath = Path.Combine(folderPath, $"{quantity}_{filename.Replace(" // ", "-")}.jpg");
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading image from the Scryfall");
        }
    }
}
