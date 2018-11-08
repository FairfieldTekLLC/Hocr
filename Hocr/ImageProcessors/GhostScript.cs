using System;
using System.Diagnostics;
using System.IO;
using Net.FairfieldTek.Hocr.Enums;
using Net.FairfieldTek.Hocr.Exceptions;

namespace Net.FairfieldTek.Hocr.ImageProcessors
{
    internal class GhostScript
    {
        private readonly int _dpi;
        private readonly string _path;

        public GhostScript(string path, int dpi)
        {
            _path = path;
            _dpi = dpi;
        }

        public string ConvertPdfToBitmap(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            try
            {
                string outPut = GetOutPutFileName(sessionName, ".bmp");
                pdf = "\"" + pdf + "\"";
                string command = string.Concat($"-dNOPAUSE -q -r{_dpi} -sDEVICE=bmp16m -dBATCH -dGraphicsAlphaBits=4 -dTextAlphaBits=4 -dFirstPage=",
                    startPageNum.ToString(), " -dLastPage=", endPageNum.ToString(), " -sOutputFile=" + outPut + " " + pdf + " -c quit");
                RunCommand(command);
                return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
            }
            catch (Exception e)
            {
                throw new GhostScriptExecuteException("Hocr.ImageProcessors.GhostScript - ConvertPdfToBitmap", e);
            }
        }

        private static string GetOutPutFileName(string sessionName, string extWithDot)
        {
            return "\"" + TempData.Instance.CreateTempFile(sessionName, extWithDot) + "\"";
        }

        public string CompressPdf(string inputPdf, string sessionName,
            PdfCompatibilityLevel level,
            dPdfSettings dPdfSettings = dPdfSettings.screen,
            string options = "")
        {
            try
            {
                string clevel;
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
                    $@"-q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=pdfwrite -dCompatibilityLevel={clevel} -dPDFSETTINGS=/{dPdfSettings} {options} -sOutputFile={'"'}{outPutFileName}{'"'} {'"'}{inputPdf}{'"'} -c quit";
                RunCommand(command);
                return outPutFileName;
            }
            catch (Exception e)
            {
                throw new GhostScriptExecuteException("Hocr.ImageProcessors.GhostScript - CompressPdf", e);
            }
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