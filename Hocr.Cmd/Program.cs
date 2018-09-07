using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hocr.Pdf;

namespace Hocr.Cmd
{
    class Program
    {
        private static bool _running = true;

        private static int FileCounter = 2;

        static async void Example(byte[] data,string outfile)
        {
            var odata = await Task.Run(() => comp.CreateSearchablePdf(data));
            File.WriteAllBytes(outfile,odata);
            Console.WriteLine("Finished " + outfile);
            FileCounter = FileCounter - 1;
            if (FileCounter == 0)
                Environment.Exit(0);

        }

        static PdfCompressor comp;

        static void Main(string[] args)
        {
            comp = new PdfCompressor(@"C:\gs\gs9.24\bin\gswin64c.exe", @"C:\Tesseract-OCR\");
            comp.OnExceptionOccurred += Compressor_OnExceptionOccurred;
            

            var data = File.ReadAllBytes(@"Test1.pdf");
            var data1 = File.ReadAllBytes(@"Test2.pdf");

            Example(data, @"Test1_ocr.pdf");
            Console.WriteLine("Started Test 1");

            Example(data1, @"Test2_ocr.pdf");
            Console.WriteLine("Starting Test 2");

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
