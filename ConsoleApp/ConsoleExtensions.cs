namespace DownloadMTGCards
{
    public static class ConsoleExtensions
    {
        public static void SetWrittingToFirstCharacter()
        {
            Console.Write(new string(' ', Console.WindowWidth - Console.CursorLeft));
        }

        public static void CleanWholeLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
