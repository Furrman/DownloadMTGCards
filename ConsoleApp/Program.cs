using Library;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var archidektClient = new ArchidektApiClient();
        var scryFallClient = new ScryfallApiClient();
        var wordGenerator = new PrintableCardsWordGenerator();

        var cardListFilename = args[0];
        var imageFolderPath = args[1];

        //var cardList = archidektClient.GetCardList("").Result;
        scryFallClient.DownloadCards(cardListFilename, imageFolderPath).Wait();
        wordGenerator.GenerateWord(imageFolderPath, Path.Combine(imageFolderPath, cardListFilename));
    }
}