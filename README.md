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
            //The folder and path for ghostscript
            const string ghostScriptPathToExecutable = @"C:\gs\gs9.24\bin\gswin64c.exe";
            //The folder where Tesseract is installed
            const string tesseractApplicationFolder = @"C:\Tesseract-OCR\";

            comp = new PdfCompressor(ghostScriptPathToExecutable, tesseractApplicationFolder);
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

Special Thanks to Koolprasadd for his original article at:  https://tech.io/playgrounds/10058/scanned-pdf-to-ocr-textsearchable-pdf-using-c
