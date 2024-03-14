using Library.Clients;
using Microsoft.Extensions.Logging;

namespace Library;

public class CardListFileParser
{
    private readonly List<string> _skippingLines = new()
    {
        "Mainboard", "Maybeboard", "Sideboard", "Commander", "Card",
        "Artifact", "Creature", "Battle", "Planeswalker", "Enchantment", "Land", "Instant", "Sorcery"
    };
    private readonly ILogger<CardListFileParser> _logger;

    public CardListFileParser(ILogger<CardListFileParser> logger)
    {
        _logger = logger;
    }

    public Dictionary<string, int> GetCardList(string filename)
    {
        var cardList = new Dictionary<string, int>();

        try
        {
            using var reader = new StreamReader(filename);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!_skippingLines.Contains(line) && !string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var cardEntries = ParseLine(line);
                if (cardEntries == null)
                {
                    continue;
                }
                cardList.Add(cardEntries.Value.Item1, cardEntries.Value.Item2);
            }
        }
        catch (Exception)
        {
        }

        return cardList;
    }


    private (string, int)? ParseLine(string line)
    {
        int quantity;
        string cardName;
        string[] parts = line.Split(' ');

        if (parts.Length >= 2)
        {
            if (int.TryParse(parts[0], out quantity))
            {
                cardName = string.Join(" ", parts, 1, parts.Length - 1);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        return (cardName, quantity);
    }
}
