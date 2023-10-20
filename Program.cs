namespace DownloadMTGCards
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client = new ScryfallApiClient();
            var filename = args[0];
            var outputPath = args[1];
            client.DownloadCards(filename, outputPath).Wait();
        }
    }
}