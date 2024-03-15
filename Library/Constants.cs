namespace Library;

internal class Constants
{
    public const string DEFAULT_FOLDER_NAME = "ArchidektPrinter_output";
    public const string DEFAULT_WORD_FILE_NAME = "ArchidektPrinter.docx";
    public const double CARD_HEIGHT = 88.0;
    public const double CARD_WIDTH = 63.0;
    public const int DPI = 96;
    public const double PIXELS_PER_MILIMETR = DPI / 25.4;
    public const double CARD_HEIGHT_PIXELS = CARD_HEIGHT * PIXELS_PER_MILIMETR;
    public const double CARD_WIDTH_PIXELS = CARD_WIDTH * PIXELS_PER_MILIMETR;
}
