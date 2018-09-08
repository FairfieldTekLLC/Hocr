using System;
using System.Diagnostics;
using System.IO;

namespace Hocr.ImageProcessors
{
    internal class GhostScript
    {
        public GhostScript(string path)
        {
            _path = path;
        }

        private readonly string _path;

        public const string Tiff12Nc = "tiff12nc";
        public const string Tiffg4 = "tiffg4";

        public string ConvertPdfToBitmap(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            string outPut = GetOutPutFileName(sessionName, ".bmp");
            pdf = "\"" + pdf + "\"";
            string command = string.Concat("-dNOPAUSE -q -r300 -sDEVICE=bmp16m -dBATCH -dFirstPage=", startPageNum.ToString(), " -dLastPage=",
                endPageNum.ToString(), " -sOutputFile=" + outPut + " " + pdf + " -c quit");
            RunCommand(command);

            return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
        }

        public string ConvertPdftoJpeg(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            string outPut = GetOutPutFileName(sessionName, ".jpeg");
            pdf = "\"" + pdf + "\"";
            string command = string.Concat("-dNOPAUSE -q -r300 -sDEVICE=jpeg -dNumRenderingThreads=8 -dBATCH -dFirstPage=", startPageNum.ToString(),
                " -dLastPage=", endPageNum.ToString(), " -sOutputFile=" + outPut + " " + pdf + " -c quit");
            RunCommand(command);

            return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
        }

        /// <summary>
        ///     Converts entire PDF to a multipage-tiff
        /// </summary>
        /// <param name="pdf"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string ConvertPdfToMultiPageTiff(string pdf, string type)
        {
            string outPut = TempData.Instance.CreateTempFile(pdf, ".tif");

            pdf = "\"" + pdf + "\"";
            string command = "-dNOPAUSE -q -r300 -sDEVICE=" + type + " -dBATCH -sOutputFile=" + outPut + " " + pdf + " -c quit";
            RunCommand(command);

            return outPut;
        }

        public string ConvertPdftoPng(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            string outPut = GetOutPutFileName(sessionName, ".png");
            pdf = "\"" + pdf + "\"";
            string command = string.Concat("-dNOPAUSE -r300 -q -dSAFER -sDEVICE=png16m -dINTERPOLATE -dNumRenderingThreads=8  -dBATCH -dFirstPage=",
                startPageNum.ToString(), " -dLastPage=", endPageNum.ToString(), " -sOutputFile=" + outPut + " " + pdf + " 30000000 setvmthreshold -c quit");
            RunCommand(command);

            return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
        }

        private static string GetOutPutFileName(string sessionName, string extWithDot)
        {
            return "\"" + TempData.Instance.CreateTempFile(sessionName, extWithDot) + "\"";
        }

        private void RunCommand(string command)
        {
            var startexe = new ProcessStartInfo(_path, command)
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using (var proc = Process.Start(startexe))
            {
                proc.WaitForExit();
            }
            GC.Collect();

        }
    }
}