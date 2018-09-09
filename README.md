## Hocr
C# Library for converting PDF files to Searchable PDF Files

* Need to batch convert 100 of scan PDF's to Searchable PFS's?
* Don't want to pay thousands of dollars for a component?



## Use Hocr!

Hocr requires:

* GhostScript to be instaled
* Windows Tesseract to be installed

I have included the installers I used when I tested this build in the "Installers" folder.
So, to run this, run the installers (or download them yourself)

* Tesseract for Windows https://github.com/UB-Mannheim/tesseract/wiki
* Ghost Script: https://www.ghostscript.com/

Example Usage:
```C#
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

        private static int FileCounter = 3;

        static async void Example(byte[] data, string outfile)
        {
            byte[] odata = await Task.Run(() => comp.CreateSearchablePdf(data, new PdfMeta()
            {
                Author = "Vince",
                KeyWords = string.Empty,
                Subject = string.Empty,
                Title = string.Empty,
            }));
            File.WriteAllBytes(outfile, odata);
            Console.WriteLine("Finished " + outfile);
            FileCounter = FileCounter - 1;
            if (FileCounter == 0)
                Environment.Exit(0);

        }

        static PdfCompressor comp;

        static void Main(string[] args)
        {
            //The folder and path for ghostscript
            const string ghostScriptPathToExecutable = @"C:\gs\gs9.24\bin\gswin64c.exe";
            //The folder where Tesseract is installed
            const string tesseractApplicationFolder = @"C:\Tesseract-OCR\";


            PdfCompressorSettings PdfSettings = new PdfCompressorSettings
            {
                ImageType = PdfImageType.JBig2,
                Dpi = 400,
                ImageQuality = 100,
                WriteTextMode = WriteTextMode.Word
            };

            comp = new PdfCompressor(ghostScriptPathToExecutable, tesseractApplicationFolder, PdfSettings);
            comp.OnExceptionOccurred += Compressor_OnExceptionOccurred;


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
```

Special Thanks to Koolprasadd for his original article at:  https://tech.io/playgrounds/10058/scanned-pdf-to-ocr-textsearchable-pdf-using-c
