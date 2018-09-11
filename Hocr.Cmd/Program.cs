using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hocr.Enums;
using Hocr.Pdf;

namespace Hocr.Cmd
{
    class Program
    {
        private static bool _running = true;

        private static int _fileCounter = 3;

        static async void Example(byte[] data, string outfile)
        {
            Tuple<byte[], string> odata = await Task.Run(() => _comp.CreateSearchablePdfAsync(data, new PdfMeta()
            {
                Author = "Vince",
                KeyWords = string.Empty,
                Subject = string.Empty,
                Title = string.Empty,
            }));
            File.WriteAllBytes(outfile, odata.Item1);
            Console.WriteLine("OCR BODY: " + odata.Item2);
            Console.WriteLine("Finished " + outfile);
            _fileCounter = _fileCounter - 1;
            if (_fileCounter == 0)
                Environment.Exit(0);

        }

        static PdfCompressor _comp;

        static void Main(string[] args)
        {
            //The folder and path for ghostscript
            const string ghostScriptPathToExecutable = @"C:\gs\gs9.24\bin\gswin64c.exe";
            //The folder where Tesseract is installed
            const string tesseractApplicationFolder = @"C:\Tesseract-OCR\";


            PdfCompressorSettings pdfSettings = new PdfCompressorSettings
            {
                ImageType = PdfImageType.Jpg,
                Dpi = 400,
                ImageQuality = 100,
                WriteTextMode = WriteTextMode.Word,
                CompressFinalPdf = true,
                PdfCompatibilityLevel = PdfCompatibilityLevel.Acrobat_7_1_6
            };

            _comp = new PdfCompressor(ghostScriptPathToExecutable, tesseractApplicationFolder, pdfSettings);
            _comp.OnExceptionOccurred += Compressor_OnExceptionOccurred;


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


        private static void Compressor_OnExceptionOccurred(PdfCompressor c, Exception x)
        {
            Console.WriteLine("Exception Occured! ");
            Console.WriteLine(x.Message);
            Console.WriteLine(x.StackTrace);
            _running = false;
        }


    }
}
