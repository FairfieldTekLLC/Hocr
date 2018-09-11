using System;
using System.Diagnostics;
using System.IO;
using Hocr.Enums;

namespace Hocr.ImageProcessors
{
    internal class GhostScript
    {
        private readonly string _path;

        public GhostScript(string path) { _path = path; }

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

        public string CompressPdf(string inputPdf, string sessionName, PdfCompatibilityLevel level)
        {
            string clevel = "1.3";
            switch (level)
            {
                case PdfCompatibilityLevel.Acrobat_4_1_3:
                    clevel = "1.3";
                    break;
                case PdfCompatibilityLevel.Acrobat_5_1_4:
                    clevel = "1.4";
                    break;
                case PdfCompatibilityLevel.Acrobat_6_1_5:
                    clevel = "1.5";
                    break;
                case PdfCompatibilityLevel.Acrobat_7_1_6:
                    clevel = "1.6";
                    break;
                case PdfCompatibilityLevel.Acrobat_7_1_7:
                    clevel = "1.7";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }


            string outPutFileName = TempData.Instance.CreateTempFile(sessionName, ".pdf");
            string command =
                $@"-q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=pdfwrite -dCompatibilityLevel={clevel} -dPDFSETTINGS=/screen -dEmbedAllFonts=true -dSubsetFonts=true -dColorImageDownsampleType=/Bicubic -dColorImageResolution=144 -dGrayImageDownsampleType=/Bicubic -dGrayImageResolution=144 -dMonoImageDownsampleType=/Bicubic -dMonoImageResolution=144 -sOutputFile={'"'}{outPutFileName}{'"'} {'"'}{inputPdf}{'"'} -c quit";
            RunCommand(command);
            return outPutFileName;
        }

        private void RunCommand(string command)
        {
            ProcessStartInfo startexe = new ProcessStartInfo(_path, command)
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using (Process proc = Process.Start(startexe))
            {
                proc?.WaitForExit();
            }
        }
    }
}