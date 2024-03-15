using Library.Clients;
using Library.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Library;

public class ArchidektPrinter
{
    private readonly ILogger<ArchidektPrinter> _logger;
    private readonly ArchidektApiClient _archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient;
    private readonly CardListFileParser _fileParser;
    private readonly WordGenerator _wordGenerator;
    private readonly FileManager _fileManager;

    public ArchidektPrinter(
        ILogger<ArchidektPrinter> logger,
        ArchidektApiClient archidektApiClient,
        ScryfallApiClient scryfallApiClient,
        CardListFileParser fileParser,
        WordGenerator wordGenerator,
        FileManager fileManager
        )
    {
        _logger = logger;
        _archidektApiClient = archidektApiClient;
        _scryfallApiClient = scryfallApiClient;
        _wordGenerator = wordGenerator;
        _fileParser = fileParser;
        _fileManager = fileManager;
    }

    public async Task SaveImages(int deckId, string? outputPath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        await SaveImages(cardList, outputPath);
    }

    public async Task SaveImages(string deckListFilePath, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        var cardList = _fileParser.GetCardList(deckListFilePath);
        await SaveImages(cardList, outputPath);
    }

    public async Task SaveImagesAndGenerateWord(int deckId, string? outputPath, string? wordFilePath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        var deckName = await _archidektApiClient.GetDeckName(deckId);
        await SaveImagesAndGenerateWord(cardList, outputPath, wordFilePath, deckName);
    }

    public async Task SaveImagesAndGenerateWord(string deckListFilePath, string? outputPath, string? wordFilePath)
    {
        var cardList = _fileParser.GetCardList(deckListFilePath);
        await SaveImagesAndGenerateWord(cardList, outputPath, wordFilePath);
    }

    public async Task SaveImagesAndGenerateWord(int deckId, string? deckListFilePath, string? outputPath, string? wordFilePath)
    {
        if (deckId != 0) await SaveImagesAndGenerateWord(deckId, outputPath, wordFilePath);
        else if (deckListFilePath != null) await SaveImagesAndGenerateWord(deckListFilePath, outputPath, wordFilePath);
        else throw new ArgumentException("DeckId has to be bigger than 0 or DeckListFilePath has to be specified");
    }

    public async Task GenerateWord(int deckId, string? wordFilePath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        var deckName = await _archidektApiClient.GetDeckName(deckId);
        await GenerateWord(cardList, wordFilePath, deckName);
    }

    public async Task GenerateWord(string deckListFilePath, string? wordFilePath)
    {
        var cardList = _fileParser.GetCardList(deckListFilePath);
        await GenerateWord(cardList, wordFilePath);
    }

    public void GenerateWordFromSavedImages(string imageFolderPath, string? wordFilePath)
    {
        if (!_fileManager.DirectoryExists(imageFolderPath))
        {
            throw new ArgumentException("ImageFolderPath has to be correct path to folder with card images");
        }
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath);
        
        _wordGenerator.GenerateWord(imageFolderPath, wordFilePath!);
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


    private async Task GenerateWord(Dictionary<string, MagicCardEntry> cardList, string? wordFilePath, string? deckName = null)
    {
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath, deckName);

        // TODO Save word at the same time of loading images from Scryfall API
    }

    private async Task SaveImages(Dictionary<string, MagicCardEntry> cardList, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        await _scryfallApiClient.DownloadCards(cardList, outputPath!);
    }

    private async Task SaveImagesAndGenerateWord(Dictionary<string, MagicCardEntry> cardList, string? imageOutputPath, string? wordFilePath, string? deckName = null)
    {
        imageOutputPath = _fileManager.CreateOutputFolder(imageOutputPath);
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath, deckName);

        await _scryfallApiClient.DownloadCards(cardList, imageOutputPath!);
        _wordGenerator.GenerateWord(imageOutputPath!, wordFilePath!);
    }
}
