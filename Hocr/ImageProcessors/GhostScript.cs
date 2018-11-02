using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using Net.FairfieldTek.Hocr.Enums;
using Net.FairfieldTek.Hocr.Util;

namespace Net.FairfieldTek.Hocr.ImageProcessors
{
    internal class GhostScript
    {

        internal class GhostScriptInstance : IDisposable
        {
            private int Innum {
                get; set;
            }
            public GhostscriptProcessor GhostscriptProcessor {
                get;
            }

            public GhostScriptInstance()
            {
                Innum = GhostScriptPool.Instance.GetInstance();
                var assembly = Assembly.GetEntryAssembly();
                string path=string.Empty;
                if (assembly != null)
                    try
                    {
                        path = Path.Combine(Path.GetDirectoryName(assembly.Location), "dlls", BuildDetect.InternalCheckIsWow64() ? $"gsdll32-{Innum}.dll" : $"gsdll64-{Innum}.dll");
                    }
                    catch (Exception e)
                    {
                        path = Path.Combine(Environment.CurrentDirectory, "dlls", BuildDetect.InternalCheckIsWow64() ? $"gsdll32-{Innum}.dll" : $"gsdll64-{Innum}.dll");
                    }

                GhostscriptProcessor = new GhostscriptProcessor(new GhostscriptLibrary(new GhostscriptVersionInfo(path)));
            }

            public void Dispose()
            {
                GhostScriptPool.Instance.GiveInstance(Innum);
                GhostscriptProcessor.Dispose();
            }
        }



        internal class GhostScriptPool
        {
            private static readonly Lazy<GhostScriptPool> lazy = new Lazy<GhostScriptPool>(() => new GhostScriptPool(5));
            public static GhostScriptPool Instance => lazy.Value;

            private readonly Stack<int> _instances = new Stack<int>();
            private GhostScriptPool(int maxInstance)
            {
                for (int i = 0; i <= maxInstance; i++)
                    _instances.Push(i);
            }

            private readonly object _locker = new object();
            public int GetInstance()
            {
                while (true)
                {
                    lock (_locker)
                    {
                        if (_instances.Count != 0)
                            return (_instances.Pop());
                        Thread.Sleep(10);
                    }

                }

            }
            public void GiveInstance(int i)
            {
                lock (_locker)
                {
                    _instances.Push(i);
                }
            }


        }

        private readonly int _dpi;


        public GhostScript(int dpi)
        {

            _dpi = dpi;
        }



        public string ConvertPdfToBitmap(string pdf, int startPageNum, int endPageNum, string sessionName)
        {

            string outPut = GetOutPutFileName(sessionName, ".bmp");
            pdf = "\"" + pdf + "\"";

            //GhostscriptProcessor proc = new GhostscriptProcessor(new GhostscriptLibrary(new GhostscriptVersionInfo(DllFile)));
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
            using (var gs = new GhostScriptInstance())
                gs.GhostscriptProcessor.Process(switches.ToArray());
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


            //GhostscriptProcessor proc = new GhostscriptProcessor(new GhostscriptLibrary(new GhostscriptVersionInfo(DllFile)));
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
            using (var gs = new GhostScriptInstance())
                gs.GhostscriptProcessor.Process(switches.ToArray());
            return outPutFileName;

        }
    }
}