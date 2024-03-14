using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using OfficeIMO.Word;

namespace Library
{
    public class PrintableCardsWordGenerator
    {
        private readonly ILogger<PrintableCardsWordGenerator> _logger;

        public PrintableCardsWordGenerator(ILogger<PrintableCardsWordGenerator> logger)
        {
            _logger = logger;
        }

        public void GenerateWord(string imageFolderPath, string? wordOutputDirectory, string? filename = null)
        {
            try
            {
                filename ??= $"{Constants.DEFAULT_FOLDER_NAME}.docx";
                if (!Path.HasExtension(filename))
                {
                    filename = $"{filename}.docx";
                }
                wordOutputDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), filename);
                var wordFilePath = Path.Combine(wordOutputDirectory, filename);
                if (!Path.Exists(wordOutputDirectory)) 
                { 
                    Directory.CreateDirectory(wordFilePath);
                }

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
            catch (Exception)
            {
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
            catch (Exception)
            {
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
            catch (Exception)
            {
            }
        }
    }
}
