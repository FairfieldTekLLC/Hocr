## Hocr
C# Library for converting PDF files to Searchable PDF Files

* Need to batch convert 100 of scan PDF's to Searchable PFS's?
* Don't want to pay thousands of dollars for a component?

I have personally tested this library with over 110 thousand PDFs.  Beyond a few fringe cases the code has performed as it was designed..  I was able to process 110k pdfs (Some hundreds of pages) over a 3 day period using 5 servers.

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
using System.Collections.Generic;
using Hocr.Enums;
using Hocr.Pdf;
using System.IO;

namespace Hocr.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Hocr.Pdf.PdfCompressor comp;
            List<string> DistillerOptions = new List<string>();
            if (1 == 1)
            {

                DistillerOptions.Add("-dSubsetFonts=true");
                DistillerOptions.Add("-dCompressFonts=true");
                DistillerOptions.Add("-sProcessColorModel=DeviceRGB");
                DistillerOptions.Add("-sColorConversionStrategy=sRGB");
                DistillerOptions.Add("-sColorConversionStrategyForImages=sRGB");
                DistillerOptions.Add("-dConvertCMYKImagesToRGB=true");
                DistillerOptions.Add("-dDetectDuplicateImages=true");
                DistillerOptions.Add("-dDownsampleColorImages=false");
                DistillerOptions.Add("-dDownsampleGrayImages=false");
                DistillerOptions.Add("-dDownsampleMonoImages=false");
                DistillerOptions.Add("-dColorImageResolution=265");
                DistillerOptions.Add("-dGrayImageResolution=265");
                DistillerOptions.Add("-dMonoImageResolution=265");
                DistillerOptions.Add("-dDoThumbnails=false");
                DistillerOptions.Add("-dCreateJobTicket=false");
                DistillerOptions.Add("-dPreserveEPSInfo=false");
                DistillerOptions.Add("-dPreserveOPIComments=false");
                DistillerOptions.Add("-dPreserveOverprintSettings=false");
                DistillerOptions.Add("-dUCRandBGInfo=/Remove");


                comp = new Hocr.Pdf.PdfCompressor("C:\\gs\\bin\\gswin32c.exe", "C:\\Tesseract-OCR\\", new PdfCompressorSettings()
                {
                    PdfCompatibilityLevel = PdfCompatibilityLevel.Acrobat_7_1_6,
                    WriteTextMode = WriteTextMode.Word,
                    Dpi = 400,
                    ImageType = PdfImageType.Tif,
                    ImageQuality = 100,
                    CompressFinalPdf = true,
                    DistillerMode = dPdfSettings.prepress,
                    DistillerOptions = string.Join(" ", DistillerOptions.ToArray())
                });
            }
            if (1 == 0)
            {
                
                DistillerOptions.Add("-dSubsetFonts=true");
                DistillerOptions.Add("-dCompressFonts=true");
                DistillerOptions.Add("-sProcessColorModel=DeviceRGB");
                DistillerOptions.Add("-sColorConversionStrategy=sRGB");
                DistillerOptions.Add("-sColorConversionStrategyForImages=sRGB");
                DistillerOptions.Add("-dConvertCMYKImagesToRGB=true");
                DistillerOptions.Add("-dDetectDuplicateImages=true");
                DistillerOptions.Add("-dDownsampleColorImages=true");
                DistillerOptions.Add("-dDownsampleGrayImages=true");
                DistillerOptions.Add("-dDownsampleMonoImages=true");
                DistillerOptions.Add("-dColorImageResolution=265");
                DistillerOptions.Add("-dGrayImageResolution=265");
                DistillerOptions.Add("-dMonoImageResolution=265");
                DistillerOptions.Add("-dDoThumbnails=false");
                DistillerOptions.Add("-dCreateJobTicket=false");
                DistillerOptions.Add("-dPreserveEPSInfo=false");
                DistillerOptions.Add("-dPreserveOPIComments=false");
                DistillerOptions.Add("-dPreserveOverprintSettings=false");
                DistillerOptions.Add("-dUCRandBGInfo=/Remove");


                comp = new Hocr.Pdf.PdfCompressor("C:\\gs\\bin\\gswin32c.exe", "C:\\Tesseract-OCR\\", new PdfCompressorSettings()
                {
                    PdfCompatibilityLevel = PdfCompatibilityLevel.Acrobat_7_1_6,
                    WriteTextMode = WriteTextMode.Word,
                    Dpi = 400,
                    ImageType = PdfImageType.Tif,
                    ImageQuality = 100,
                    CompressFinalPdf = true,
                    DistillerMode = dPdfSettings.prepress,
                    DistillerOptions = string.Join(" ", DistillerOptions.ToArray())
                });
            }

            if (1 == 0)
            {

                DistillerOptions.Add("-dSubsetFonts=true");
                DistillerOptions.Add("-dCompressFonts=true");
                DistillerOptions.Add("-sProcessColorModel=DeviceCMYK");
                DistillerOptions.Add("-dUseCIEColor=true");

                DistillerOptions.Add("-sColorConversionStrategy=sLeaveColorUnchanged");

                DistillerOptions.Add("-sColorConversionStrategyForImages=sLeaveColorUnchanged");

                DistillerOptions.Add("-dConvertCMYKImagesToRGB=false");

                DistillerOptions.Add("-dDetectDuplicateImages=true");

                DistillerOptions.Add("-dDownsampleColorImages=false");

                DistillerOptions.Add("-dDownsampleGrayImages=false");

                DistillerOptions.Add("-dDownsampleMonoImages=false");

                DistillerOptions.Add("-dColorImageResolution=300");

                DistillerOptions.Add("-dGrayImageResolution=300");

                DistillerOptions.Add("-dMonoImageResolution=300");

                DistillerOptions.Add("-dDoThumbnails=false");

                DistillerOptions.Add("-dCreateJobTicket=false");

                DistillerOptions.Add("-dPreserveEPSInfo=false");

                DistillerOptions.Add("-dPreserveOPIComments=false");

                DistillerOptions.Add("-dPreserveOverprintSettings=false");

                DistillerOptions.Add("-dUCRandBGInfo=/Remove");


                comp = new Hocr.Pdf.PdfCompressor("C:\\gs\\bin\\gswin32c.exe", "C:\\Tesseract-OCR\\", new PdfCompressorSettings()
                {
                    PdfCompatibilityLevel = PdfCompatibilityLevel.Acrobat_7_1_6,
                    WriteTextMode = WriteTextMode.Word,
                    Dpi = 300,
                    ImageType = PdfImageType.Jpg,
                    ImageQuality = 100,
                    CompressFinalPdf = true,
                    DistillerMode = dPdfSettings.prepress,
                    DistillerOptions = string.Join(" ", DistillerOptions.ToArray())
                });
            }


            comp.OnCompressorEvent += Comp_OnCompressorEvent;

            foreach (string file in Directory.GetFiles("C:\\pdfin"))
            {
                byte[] data = File.ReadAllBytes(file);
                System.Tuple<byte[], string> result = comp.CreateSearchablePdf(data, new PdfMeta());
                File.WriteAllBytes("c:\\PDFOUT\\" + Path.GetFileName(file), result.Item1);
            }

        }

        private static void Comp_OnCompressorEvent(string msg) { System.Console.WriteLine(msg); }
    }
}

```

Special Thanks to Koolprasadd for his original article at:  https://tech.io/playgrounds/10058/scanned-pdf-to-ocr-textsearchable-pdf-using-c
