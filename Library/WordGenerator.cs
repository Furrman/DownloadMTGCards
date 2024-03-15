using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using OfficeIMO.Word;

namespace Library;

public class WordGenerator
{
    private readonly ILogger<WordGenerator> _logger;

    public WordGenerator(ILogger<WordGenerator> logger)
    {
        _logger = logger;
    }

    public void GenerateWord(string imageFolderPath, string wordFilePath)
    {
        try
        {
            using WordDocument document = WordDocument.Create(wordFilePath);

            document.Margins.Type = WordMargin.Narrow;
            document.PageSettings.Orientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;

            var paragraph = document.AddParagraph();

            var imageFiles = Directory.GetFiles(imageFolderPath, "*.jpg");
            foreach (string imagePath in imageFiles)
            {
                AddImageToWord(paragraph, imagePath);
            }

            SaveWord(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in writting images to Word file");
        }
    }

    private void AddImageToWord(WordParagraph paragraph, string imagePath)
    {
        try
        {
            double targetHeight = 88.0;
            double targetWidth = 63.0;
            int dpi = 96;

            double pixelsPerMillimeter = dpi / 25.4;
            double heightPixels = targetHeight * pixelsPerMillimeter;
            double widthPixels = targetWidth * pixelsPerMillimeter;

            var cardFile = Path.GetFileNameWithoutExtension(imagePath);
            ExtractPattern(cardFile, out int cardQuantity, out string cardName);

            for (int i = 0; i < cardQuantity; i++)
            {
                paragraph.AddImage(imagePath, width: widthPixels, height: heightPixels);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in adding image to word file");
        }
    }
    
    private void ExtractPattern(string input, out int intValue, out string stringValue)
    {
        intValue = -1;
        stringValue = string.Empty;

        int underscoreIndex = input.IndexOf('_');
        if (underscoreIndex != -1 && underscoreIndex < input.Length - 1)
        {
            string intPart = input.Substring(0, underscoreIndex);
            if (int.TryParse(intPart, out intValue))
            {
                stringValue = input.Substring(underscoreIndex + 1);
            }
        }
    }

    private void SaveWord(WordDocument document)
    {
        try
        {
            document.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in saving Word file");
        }
    }
}
