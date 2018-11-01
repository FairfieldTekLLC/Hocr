using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Net.FairfieldTek.Hocr.HocrElements;
using Net.FairfieldTek.Hocr.ImageProcessors;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Net.FairfieldTek.Hocr
{
    internal class OcrController
    {
        internal void AddToDocument(string language, Image image, ref HDocument doc, string sessionName)
        {
            Bitmap b = ImageProcessor.GetAsBitmap(image, (int) Math.Ceiling(image.HorizontalResolution));
            string imageFile = TempData.Instance.CreateTempFile(sessionName, ".tif");
            b.Save(imageFile, ImageFormat.Tiff);
            string result = CreateHocr(language, imageFile, sessionName);
            doc.AddFile(result);
            b.Dispose();
        }

        public string CreateHocr(string language, string imagePath, string sessionName)
        {
            string dataFolder = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            string dataPath = TempData.Instance.CreateDirectory(sessionName, dataFolder);
            string outputFile = Path.Combine(dataPath, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            string enginePath = string.Empty;
            try
            {
                enginePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? throw new InvalidOperationException(), "tessdata");
            }
            catch (Exception e)
            {
                enginePath = Path.Combine(Environment.CurrentDirectory, "tessdata");
            }






            using (TesseractEngine engine = new TesseractEngine(enginePath, "eng"))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        string hocrtext = page.GetHOCRText(0);
                        File.WriteAllText(outputFile + ".hocr", hocrtext);
                    }
                }
            }
            return outputFile + ".hocr";
        }
    }
}