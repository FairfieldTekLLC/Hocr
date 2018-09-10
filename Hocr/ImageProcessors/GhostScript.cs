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

        private static string GetOutPutFileName(string sessionName, string extWithDot)
        {
            return "\"" + TempData.Instance.CreateTempFile(sessionName, extWithDot) + "\"";
        }

        public string CompressPdf(string inputPdf, string sessionName)
        {
            string outPutFileName = TempData.Instance.CreateTempFile(sessionName, ".pdf");
            string command =$@"-q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=pdfwrite -dCompatibilityLevel=1.3 -dPDFSETTINGS=/screen -dEmbedAllFonts=true -dSubsetFonts=true -dColorImageDownsampleType=/Bicubic -dColorImageResolution=144 -dGrayImageDownsampleType=/Bicubic -dGrayImageResolution=144 -dMonoImageDownsampleType=/Bicubic -dMonoImageResolution=144 -sOutputFile={'"'}{outPutFileName}{'"'} {'"'}{inputPdf}{'"'} -c quit";
            RunCommand(command);
            return outPutFileName;

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
        }
    }
}