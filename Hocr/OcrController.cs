using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Hocr.HocrElements;
using Hocr.ImageProcessors;

namespace Hocr
{
    internal class OcrController
    {
        private readonly string _path;
        public OcrController(string path)
        {
            _path = path;
        }

        internal void AddToDocument(string language, Image image, ref HDocument doc, string sessionName)
        {
            Bitmap b = ImageProcessor.GetAsBitmap(image, (int)Math.Ceiling(image.HorizontalResolution));


            string imageFile = TempData.Instance.CreateTempFile(sessionName, ".tif");
            b.Save(imageFile, ImageFormat.Tiff);


            string result = CreateHocr(language, imageFile, sessionName);
            doc.AddFile(result);
        }

        public string CreateHocr(string language, string imagePath, string sessionName)
        {
            string dataFolder = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            dataFolder = dataFolder + "/tessData";
            string dataPath = TempData.Instance.CreateDirectory(sessionName, dataFolder);
            string outputFile = imagePath.Replace(Path.GetExtension(imagePath), ".hocr");
            string inputFile = string.Concat('"', imagePath, '"');
            const string processName = "tesseract";
            string oArg = '"' + outputFile + '"';
            string commandArgs = string.Concat(inputFile, " ", oArg, " -l " + language + " -psm 1 hocr --oem 2 --tessdata-dir " + dataPath);
            RunCommand(processName, commandArgs);
            return outputFile + ".hocr";
        }
        
        private void RunCommand(string processName, string commandArgs)
        {
            var startexe = new ProcessStartInfo(processName, commandArgs)
            {
                WorkingDirectory = _path,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = true,
            };
            using (Process proc = Process.Start(startexe))
            {
                proc.WaitForExit();
            }
        }
    }
}
