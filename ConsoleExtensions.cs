namespace DownloadMTGCards
{
    public static class ConsoleExtensions
    {
        public static void ClearLastLine()
        {
            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
        }
    }
}
