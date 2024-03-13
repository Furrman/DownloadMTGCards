namespace DownloadMTGCards;

internal class Program
{
    static void Main(string[] args)
    {
        var client = new ScryfallApiClient();
        var wordGenerator = new PrintableCardsWordGenerator();
        var cardListFilename = args[0];
        var imageFolderPath = args[1];
        client.DownloadCards(cardListFilename, imageFolderPath).Wait();
        wordGenerator.GenerateWord(imageFolderPath, Path.Combine(imageFolderPath, cardListFilename));
    }
}