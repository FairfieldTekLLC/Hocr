using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using Image = iTextSharp.text.Image;

namespace Hocr.ImageProcessors
{
    internal class JBig2
    {
        public string JBig2Path =>
            '"' + Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), "jbig2.exe") + '"';

        private void CompressJBig2(string imagePath)
        {
            var startexe = new ProcessStartInfo()
            {
                FileName = JBig2Path,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                Arguments = "-d -p -s -S -b " + '"' + imagePath.Replace(Path.GetExtension(imagePath), string.Empty) + '"' + " " + '"' + imagePath + '"'
            };

            using (var proc = Process.Start(startexe))
            {
                proc?.WaitForExit();
            }
        }

        public Image ProcessImage(string imagePath)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);
            string newImage = imagePath;
            string symFilePath = newImage.Replace(Path.GetExtension(newImage), ".sym");
            string symIndexFilePath = newImage.Replace(Path.GetExtension(newImage), ".0000");
            CompressJBig2(newImage);
            Image i = Image.GetInstance(img.Width, img.Height, File.ReadAllBytes(symIndexFilePath), File.ReadAllBytes(symFilePath));
            return i;
        }

        public Image ProcessImage(Bitmap b, string sessionName)
        {
            Bitmap img = ImageProcessor.GetAsBitmap(b, (int)b.HorizontalResolution);
            string s = TempData.Instance.CreateTempFile(sessionName, ".bmp");
            img.Save(s);
            return ProcessImage(s);
        }
    }
}