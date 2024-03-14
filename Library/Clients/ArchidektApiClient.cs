using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Library.Clients;

public class ArchidektApiClient
{
    private readonly string _baseUrl = "https://archidekt.com/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArchidektApiClient> _logger;

    public ArchidektApiClient(ILogger<ArchidektApiClient> logger)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _logger = logger;
    }

    public async Task<Dictionary<string, int>> GetCardList(int deckId)
    {
        var cardList = new Dictionary<string, int>();

        var requestUrl = $"/api/deck/{deckId}/";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in getting card list from the deck", deckId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeckId: {deckId} Error in parsing card list from the deck", deckId);
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
            _logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck", deckId);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in getting deck name", deckId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeckId: {deckId} Error in parsing response with deck name", deckId);
                return deckName;
            }

            deckName = jsonObject?["name"]?.Value<string>()!;
        }
        else
        {
            _logger.LogWarning("DeckId: {deckId} Failure response from getting deck name {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
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
