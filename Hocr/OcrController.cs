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
        public OcrController(string path) { _path = path; }
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
            const string processName = "tesseract.exe";
            string commandArgs = $" {'"'}{imagePath}{'"'} {'"'}{outputFile}{'"'} -l {language} -psm 1 hocr ";
            RunCommand(processName, commandArgs);
            return outputFile + ".hocr";
        }
        private void RunCommand(string processName, string commandArgs)
        {
            string pgmExec = Path.Combine(_path, processName);
            try
            {
                ProcessStartInfo startexe = new ProcessStartInfo(pgmExec, commandArgs)
                {
                    WorkingDirectory = _path,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = true
                };
                using (Process proc = Process.Start(startexe))
                {
                    proc?.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
    }
}