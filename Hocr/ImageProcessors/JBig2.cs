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
        private string jbig2Path;

        public JBig2()
        {
            jbig2Path = JBig2Path;
        }

        public JBig2(string jbig2EXEPath)
        {
            jbig2Path = jbig2EXEPath;
        }

        public string JBig2Path {
            get {


                string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                return '"' + Path.Combine(applicationDirectory, "jbig2.exe") + '"';

            }
        }

        private void Cleanup(string f)
        {
            if (File.Exists(f))
                try
                {
                    File.Delete(f);
                }
                catch (Exception x)
                {
                }
        }

        private void CompressJBig2(string imagePath)
        {
            Process p = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = JBig2Path;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.CreateNoWindow = true;
            //-t .75
            info.Arguments = "-d -p -s -S -b " + '"' + imagePath.Replace(Path.GetExtension(imagePath), string.Empty) + '"' + " " + '"' + imagePath + '"';
            p.StartInfo = info;
            try
            {
                p.Start();
                p.WaitForExit();
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.Message);
                throw x;
            }
        }

        public Image ProcessImage(string imagePath)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(imagePath);
            //Bitmap bmp = new Bitmap(img.Width, img.Height);
            //Graphics gImage = Graphics.FromImage(bmp);
            //gImage.DrawImage(img, 0, 0, img.Width, img.Height);

            //ImageFormat saveToFormat = ImageFormat.Jpeg;

            string newImage = imagePath; // TempData.Instance.CreateTempFile("." + saveToFormat.ToString().ToLower());

            //bmp.SetResolution(300, 300);
            //bmp.Save(newImage, saveToFormat);

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