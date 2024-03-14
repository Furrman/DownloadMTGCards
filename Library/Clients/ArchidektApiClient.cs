using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using OfficeIMO.Word;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace Library.Clients;

public class ArchidektApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://archidekt.com/";

    public ArchidektApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    public async Task<Dictionary<string, int>> GetCardList(int deckId)
    {
        var cardList = new Dictionary<string, int>();

        var requestUrl = $"/api/decks/{deckId}/";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception)
        {
            return cardList;
        }

        if (response.IsSuccessStatusCode)
        {
            JObject jsonObject;
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                jsonObject = JObject.Parse(json);
            }
            catch
            {
                return cardList;
            }

            var cards = jsonObject?["cards"];
            if (cards is null || cards?.Count() == 0)
            {
                return cardList;
            }

            foreach (var card in cards!)
            {
                var cardName = card?["card"]?["oracleCard"]?["name"]?.ToString();
                if (cardName is null)
                {
                    continue;
                }
                if (!int.TryParse(card?["quantity"]?.ToString(), out var cardQuantity))
                {
                    continue;
                }

                if (!cardList.TryAdd(cardName, cardQuantity))
                {
                    var value = cardList.GetValueOrDefault(cardName);
                    value++;
                    cardList[cardName] = value;
                }
            }
        }
        else
        {
        }

        return cardList;
    }

    public async Task<string> GetDeckName(int deckId)
    {
        string deckName = string.Empty;

        var requestUrl = $"/api/decks/{deckId}/";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception)
        {
            return deckName;
        }

        if (response.IsSuccessStatusCode)
        {
            JObject jsonObject;
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                jsonObject = JObject.Parse(json);
            }
            catch
            {
                return deckName;
            }

            deckName = jsonObject?["name"]?.Value<string>()!;
        }
        else
        {
        }

        return deckName;
    }

    public bool TryExtractDeckIdFromUrl(string url, out int deckId)
    {
        deckId = 0;
        string pattern = @"^https:\/\/archidekt\.com\/(?:api\/decks\/(\d+)\/|decks\/(\d+)\/)";
        Regex regex = new(pattern);

        Match match = regex.Match(url);
        if (match.Success)
        {
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (int.TryParse(match.Groups[i].Value, out deckId))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
