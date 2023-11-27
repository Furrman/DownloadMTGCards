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
        var fileNameWithoutExtension = Path.ChangeExtension(filename, null);
        var cardNames = GetCardNames(filename);
        outputPath ??= CreateFolderForPictures(fileNameWithoutExtension);

        int step = 1;
        int count = cardNames.Count;
        foreach (var cardName in cardNames)
        {
            double percentage = (step / (double)count) * 100;
            Console.Write($"\rProgress: {percentage:F2}%");
            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
            step++;

            var imageUrl = await GetCardUrlFromScryfall(cardName);
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await DownloadImage(imageUrl, outputPath, cardName);
            }
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
                lines.Add(line);
            }
        }
        catch (Exception e)
        {
            _errors.Add($"Card: {filename} Error: {e.Message}");
        }

        return lines;
    }

    private async Task<string?> GetCardUrlFromScryfall(string cardName)
    {
        string requestUrl = "/cards/search?q=" + cardName;

        var response = await _httpClient.GetAsync(requestUrl);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(json);
            // TODO work with dual side cards as well
            var imageUrl = jsonObject?["data"]?[0]?["image_uris"]?["large"]?.Value<string>();

            return imageUrl;
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
