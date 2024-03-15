using Library.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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

    public async Task<Dictionary<string, MagicCardEntry>> GetCardList(int deckId)
    {
        var cardList = new Dictionary<string, MagicCardEntry>();

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

                if (cardList.ContainsKey(cardName))
                {
                    cardList[cardName].Quantity++;
                }
                else
                {
                    cardList.Add(cardName, new MagicCardEntry
                    {
                        Name = cardName,
                        Quantity = cardQuantity
                    });
                }
            }
        }
        else
        {
            _logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
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
            _logger.LogWarning("DeckId: {deckId} Failure response from getting deck name Request: {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
        }

        return deckName;
    }
}
