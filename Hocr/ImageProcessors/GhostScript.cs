using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using Net.FairfieldTek.Hocr.Enums;
using Net.FairfieldTek.Hocr.Util;

namespace Net.FairfieldTek.Hocr.ImageProcessors
{
    internal class GhostScript
    {
        private readonly int _dpi;
        

        public GhostScript( int dpi)
        {
        
            _dpi = dpi;
        }

        private static string DllFile {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly!=null && assembly.Location!=null)
                    try
                    {
                        return Path.Combine(Path.GetDirectoryName(assembly.Location), "dlls", BuildDetect.InternalCheckIsWow64() ? "gsdll32.dll" : "gsdll64.dll");
                    }
                    catch (Exception e)
                    {
                    }
                    
                return Path.Combine( Environment.CurrentDirectory,"dlls", BuildDetect.InternalCheckIsWow64() ? "gsdll32.dll" : "gsdll64.dll");
            }
    } 

        public string ConvertPdfToBitmap(string pdf, int startPageNum, int endPageNum, string sessionName)
        {
            string outPut = GetOutPutFileName(sessionName, ".bmp");
            pdf = "\"" + pdf + "\"";

            GhostscriptProcessor proc = new GhostscriptProcessor(new GhostscriptLibrary(new GhostscriptVersionInfo(DllFile)));
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-q",
                $"-r{_dpi}",
                "-sDEVICE=bmp16m",
                "-dBATCH",
                "-dGraphicsAlphaBits=4",
                "-dTextAlphaBits=4",
                $"-dFirstPage={startPageNum}",
                $"-dLastPage={endPageNum}",
                $"-sOutputFile={outPut}",
                pdf,
                "-c",
                "quit"
            };
            proc.Process(switches.ToArray());
            return new FileInfo(outPut.Replace('"', ' ').Trim()).FullName;
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


            GhostscriptProcessor proc = new GhostscriptProcessor(new GhostscriptLibrary(new GhostscriptVersionInfo(DllFile)));
            List<string> switches = new List<string>
            {
                "-q",
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                $"-dCompatibilityLevel={clevel}",
                $"-dPDFSETTINGS=/{dPdfSettings}",
                options,
                $"-sOutputFile={'"'}{outPutFileName}{'"'}",
                $"{'"'}{inputPdf}{'"'}",
                "-c",
                "quit"
            };
            proc.Process(switches.ToArray());
            return outPutFileName;
        }
    }
}