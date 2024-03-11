using Newtonsoft.Json.Linq;
using System.Net;

namespace DownloadMTGCards;

public class ScryfallApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://api.scryfall.com";
    private List<string> _errors = new();

    public ScryfallApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    public async Task DownloadCards(string filename, string? outputPath = null)
    {
        try
        {
            var fileNameWithoutExtension = Path.ChangeExtension(filename, null);
            var cardNames = GetCardNames(filename);
            outputPath ??= fileNameWithoutExtension;
            outputPath = CreateFolderForPictures(outputPath);

            int step = 1;
            int count = cardNames.Count;
            foreach (var cardName in cardNames)
            {
                double percentage = (step / (double)count) * 100;
                Console.Write($"\rProgress: {percentage:F2}%");
                Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
                step++;

                var images = await GetCardImageUrlsFromScryfall(cardName);
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        await DownloadImage(image.Value, outputPath, image.Key);
                    }
                }
            }
        }
        catch( Exception ex)
        {
            _errors.Add(ex.Message);
        }

        Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
        foreach (var error in _errors)
        {
            Console.WriteLine(error);
        }
        _errors.Clear();
    }

    private List<string> GetCardNames(string filename)
    {
        var lines = new List<string>();

        try
        {
            using var reader = new StreamReader(filename);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(RemoveFirstNumberAndSpace(line));
            }
        }
        catch (Exception e)
        {
            _errors.Add($"Card: {filename} Error: {e.Message}");
        }

        var skippingLines = new List<string>
        {
            "Mainboard", "Maybeboard", "Sideboard", "Commander", "Card",
            "Artifact", "Creature", "Battle", "Planeswalker", "Enchantment", "Land", "Instant", "Sorcery"
        };
        lines = lines
            .Where(l => !skippingLines.Contains(l) && !string.IsNullOrWhiteSpace(l))
            .ToList();

        return lines;
    }

    private static string RemoveFirstNumberAndSpace(string input)
    {
        // Find the index of the first space
        int spaceIndex = input.IndexOf(' ');

        // If space exists, remove the characters before it
        if (spaceIndex != -1)
        {
            return input.Substring(spaceIndex + 1);
        }

        // If no space, return the input as is
        return input;
    }

    private async Task<Dictionary<string, string>?> GetCardImageUrlsFromScryfall(string cardName)
    {
        string requestUrl = "/cards/search?q=" + cardName;

        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var jsonObject = JObject.Parse(json);
            Dictionary<string, string> imagesUrl = new();
            if (jsonObject?["data"] is null || jsonObject?["data"]?.Count() == 0)
            {
                _errors.Add($"Card: {cardName} not found");
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
            else
            {
                imagesUrl.Add(cardName, jsonObject?["data"]?[index]?["image_uris"]?["large"]?.Value<string>()!);
            }

            return imagesUrl;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _errors.Add($"Card: {cardName} not found");
            return null;
        }
        else
        {
            _errors.Add($"Card: {cardName} HttpError: {response.StatusCode}");
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
        }
    }

    private async Task DownloadImage(string imageUrl, string folderPath, string filename)
    {
        using var client = new HttpClient();
        try
        {
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);

            var filePath = Path.Combine(folderPath, $"{filename}.jpg");
            File.WriteAllBytes(filePath, imageBytes);
        }
        catch (Exception e)
        {
            _errors.Add($"Card: {filename} Error: {e.Message}");
        }
    }

    private string CreateFolderForPictures(string directoryName)
    {
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        return Path.GetFullPath(directoryName);
    }
}
