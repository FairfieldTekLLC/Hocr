using Hocr.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace Hocr.ImageProcessors
{
    internal class GhostScript
    {
        private readonly string _path;
        private readonly int _dpi;

        public GhostScript(string path, int dpi)
        {
            _path = path;
            _dpi = dpi;
        }

        public string ConvertPdfToBitmap(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            string outPut = GetOutPutFileName(sessionName, ".bmp");
            pdf = "\"" + pdf + "\"";


            string command = string.Concat($"-dNOPAUSE -q -r{_dpi} -sDEVICE=bmp16m -dBATCH -dGraphicsAlphaBits=4 -dTextAlphaBits=4 -dFirstPage=", startPageNum.ToString(), " -dLastPage=",endPageNum.ToString(), " -sOutputFile=" + outPut + " " + pdf + " -c quit");
            
            RunCommand(command);
            return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
        }

        private static string GetOutPutFileName(string sessionName, string extWithDot)
        {
            return "\"" + TempData.Instance.CreateTempFile(sessionName, extWithDot) + "\"";
        }

        //public string CompressPdf(string inputPdf, string sessionName, 
        //    PdfCompatibilityLevel level,
        //    dImageDownSampleType dImageDownSampleType = dImageDownSampleType.Bicubic,
        //    dPdfSettings dPdfSettings = dPdfSettings.screen, 
        //    int imageResolutionFactor = 2)
        //{
        //    string clevel;
        //    switch (level)
        //    {
        //        case PdfCompatibilityLevel.Acrobat_4_1_3:
        //            clevel = "1.3";
        //            break;
        //        case PdfCompatibilityLevel.Acrobat_5_1_4:
        //            clevel = "1.4";
        //            break;
        //        case PdfCompatibilityLevel.Acrobat_6_1_5:
        //            clevel = "1.5";
        //            break;
        //        case PdfCompatibilityLevel.Acrobat_7_1_6:
        //            clevel = "1.6";
        //            break;
        //        case PdfCompatibilityLevel.Acrobat_7_1_7:
        //            clevel = "1.7";
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(level), level, null);
        //    }

        //    int imageResolution = 72 * imageResolutionFactor;



        //    string outPutFileName = TempData.Instance.CreateTempFile(sessionName, ".pdf");
        //    string command = $@"-q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=pdfwrite -dCompatibilityLevel={clevel} -dPDFSETTINGS=/{dPdfSettings} -dEmbedAllFonts=true -dSubsetFonts=true -dColorImageDownsampleType=/{dImageDownSampleType} -dColorImageResolution={imageResolution} -dGrayImageDownsampleType=/{dImageDownSampleType} -dGrayImageResolution={imageResolution} -dMonoImageDownsampleType=/{dImageDownSampleType} -dMonoImageResolution={imageResolution} -sOutputFile={'"'}{outPutFileName}{'"'} {'"'}{inputPdf}{'"'} -c quit";
        //    RunCommand(command);
        //    return outPutFileName;
        //}

        public string CompressPdf(string inputPdf, string sessionName,
            PdfCompatibilityLevel level,
            dPdfSettings dPdfSettings = dPdfSettings.screen,
            string options="")
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

            //-dMonoImageDownsampleType=/{dImageDownSampleType} -dMonoImageResolution={imageResolution} 

            string outPutFileName = TempData.Instance.CreateTempFile(sessionName, ".pdf");
            string command = $@"-q -dNOPAUSE -dBATCH -dSAFER -sDEVICE=pdfwrite -dCompatibilityLevel={clevel} -dPDFSETTINGS=/{dPdfSettings} {options} -sOutputFile={'"'}{outPutFileName}{'"'} {'"'}{inputPdf}{'"'} -c quit";
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