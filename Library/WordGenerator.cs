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


    public async Task GenerateWord(string wordFilePath, Func<WordDocument, Task>? customAction = null)
    {
        try
        {
            using WordDocument document = WordDocument.Create(wordFilePath);

            document.Margins.Type = WordMargin.Narrow;
            document.PageSettings.Orientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;

            if (customAction != null)
            {
                await customAction!.Invoke(document);
            }

            document.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in writting images to Word file");
        }
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

            document.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in writting images to Word file");
        }
    }

    public void AddImageToWord(WordParagraph paragraph, string imageName, byte[] imageContent, int quantity)
    {
        try
        {
            using MemoryStream stream = new(imageContent);
            paragraph.AddImage(stream, imageName, width: Constants.CARD_WIDTH_PIXELS, height: Constants.CARD_HEIGHT_PIXELS);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in adding image to word file");
        }
    }


    private void AddImageToWord(WordParagraph paragraph, string imagePath)
    {
        try
        {
            var cardFile = Path.GetFileNameWithoutExtension(imagePath);
            ExtractPattern(cardFile, out int cardQuantity, out string cardName);

            for (int i = 0; i < cardQuantity; i++)
            {
                paragraph.AddImage(imagePath, width: Constants.CARD_WIDTH_PIXELS, height: Constants.CARD_HEIGHT_PIXELS);
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
}
