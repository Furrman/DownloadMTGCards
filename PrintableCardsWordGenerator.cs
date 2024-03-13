using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;

namespace DownloadMTGCards
{
    public class PrintableCardsWordGenerator
    {
        private List<string> _errors = new();

        public void GenerateWord(string folderPath, string outputPath)
        {
            try
            {
                var wordFilePath = Path.ChangeExtension(outputPath, "docx");
                using WordDocument document = WordDocument.Create(wordFilePath);

                document.Margins.Type = WordMargin.Narrow;
                document.PageSettings.Orientation = PageOrientationValues.Landscape;
                document.PageSettings.PageSize = WordPageSize.A4;

                var paragraph = document.AddParagraph();

                // Get all image files in the folder
                string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");
                int step = 0;
                Console.Write($"\rImages progress: {0:F2}%");
                ConsoleExtensions.ClearLastLine();

                foreach (string imagePath in imageFiles)
                {
                    AddImageToWord(paragraph, imagePath);

                    double percentage = (step / (double)imageFiles.Length) * 100;
                    Console.Write($"\rImages progress: {percentage:F2}%");
                    ConsoleExtensions.ClearLastLine();
                    step++;
                }
                Console.Write($"\rImages progress: {100:F2}%");
                Console.Write($"\rSaving word file...");

                SaveWord(document, wordFilePath);

                Console.Write($"\rSaving to word completed!");
            }
            catch (Exception ex)
            {
                _errors.Add($"Document: {outputPath} Error: {ex.Message}");
            }

            if (_errors.Count > 0)
            {
                Console.WriteLine("Errors:");
            }
            foreach (var error in _errors)
            {
                Console.WriteLine(error);
            }
        }

        private void AddImageToWord(WordParagraph paragraph, string imagePath)
        {
            try
            {
                double targetHeight = 88.0;
                double targetWidth = 0.0;
                using (var image = System.Drawing.Image.FromFile(imagePath))
                {
                    var aspectRatio = (double)image.Width / image.Height;
                    targetWidth = targetHeight * aspectRatio;
                }
                int dpi = 96;
                double pixelsPerMillimeter = dpi / 25.4;
                double heightPixels = targetHeight * pixelsPerMillimeter;
                double widthPixels = targetWidth * pixelsPerMillimeter;
                paragraph.AddImage(imagePath, width: widthPixels, height: heightPixels);
            }
            catch (Exception ex)
            {
                _errors.Add($"File: {imagePath} Error: {ex.Message}");
            }
        }

        private void SaveWord(WordDocument document, string outputPath)
        {
            try
            {
                document.Save();
            }
            catch (Exception e)
            {
                _errors.Add($"Document: {outputPath} Error: {e.Message}");
            }
        }
    }
}
