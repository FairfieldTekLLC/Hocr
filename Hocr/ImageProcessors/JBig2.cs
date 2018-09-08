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

        private static void Cleanup(string f)
        {
            if (!File.Exists(f))
                return;
            try
            {
                File.Delete(f);
            }
            catch (Exception x)
            {
                //
            }
        }

        private void CompressJBig2(string imagePath)
        {
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = JBig2Path,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                Arguments = "-d -p -s -S -b " + '"' + imagePath.Replace(Path.GetExtension(imagePath), string.Empty) + '"' + " " + '"' + imagePath + '"'
            };
            p.StartInfo = info;
            try
            {
                p.Start();
                p.WaitForExit();
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.Message);
                throw;
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
            Cleanup(symFilePath);
            Cleanup(symIndexFilePath);
            Cleanup(newImage);
            GC.Collect();
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