using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Hocr.Enums;
using Hocr.Pdf;

namespace Hocr.Cmd
{
    internal class Program
    {
        private static bool _running = true;

        private static int _fileCounter = 3;

        private static PdfCompressor _comp;

        public static void Example(byte[] data, string outfile)
        {
            Tuple<byte[], string> odata = _comp.CreateSearchablePdf(data, new PdfMeta
            {
                Author = "Vince",
                KeyWords = string.Empty,
                Subject = string.Empty,
                Title = string.Empty
            });
            File.WriteAllBytes(outfile, odata.Item1);
            Console.WriteLine("OCR BODY: " + odata.Item2);
            Console.WriteLine("Finished " + outfile);
            _fileCounter = _fileCounter - 1;
            if (_fileCounter == 0)
                Environment.Exit(0);
        }

        private static void Main(string[] args)
        {
            List<string> distillerOptions = new List<string>
            {
                "-dSubsetFonts=true",
                "-dCompressFonts=true",
                "-sProcessColorModel=DeviceRGB",
                "-sColorConversionStrategy=sRGB",
                "-sColorConversionStrategyForImages=sRGB",
                "-dConvertCMYKImagesToRGB=true",
                "-dDetectDuplicateImages=true",
                "-dDownsampleColorImages=false",
                "-dDownsampleGrayImages=false",
                "-dDownsampleMonoImages=false",
                "-dColorImageResolution=265",
                "-dGrayImageResolution=265",
                "-dMonoImageResolution=265",
                "-dDoThumbnails=false",
                "-dCreateJobTicket=false",
                "-dPreserveEPSInfo=false",
                "-dPreserveOPIComments=false",
                "-dPreserveOverprintSettings=false",
                "-dUCRandBGInfo=/Remove"
            };


            PdfCompressorSettings pdfSettings = new PdfCompressorSettings
            {
                PdfCompatibilityLevel = PdfCompatibilityLevel.Acrobat_7_1_6,
                WriteTextMode = WriteTextMode.Word,
                Dpi = 400,
                ImageType = PdfImageType.Tif,
                ImageQuality = 100,
                CompressFinalPdf = true,
                DistillerMode = dPdfSettings.prepress,
                DistillerOptions = string.Join(" ", distillerOptions.ToArray())
            };

            _comp = new PdfCompressor(pdfSettings);
            _comp.OnExceptionOccurred += Compressor_OnExceptionOccurred;
            _comp.OnCompressorEvent += _comp_OnCompressorEvent;


            byte[] data = File.ReadAllBytes(@"Test1.pdf");
            byte[] data1 = File.ReadAllBytes(@"Test2.pdf");
            byte[] data2 = File.ReadAllBytes(@"Test3.pdf");

            Example(data, @"Test1_ocr.pdf");
            Console.WriteLine("Started Test 1");

            Example(data1, @"Test2_ocr.pdf");
            Console.WriteLine("Starting Test 2");

            Example(data2, @"Test3_ocr.pdf");
            Console.WriteLine("Starting Test 3");

            int counter = 0;
            while (_running)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Working...." + counter);
                counter++;
            }

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        private static void _comp_OnCompressorEvent(string msg) { Console.WriteLine(msg); }

        private static void Compressor_OnExceptionOccurred(PdfCompressor c, Exception x)
        {
            Console.WriteLine("Exception Occured! ");
            Console.WriteLine(x.Message);
            Console.WriteLine(x.StackTrace);
            _running = false;
        }
    }
}