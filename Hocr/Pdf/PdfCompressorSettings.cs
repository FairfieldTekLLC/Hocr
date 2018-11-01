using iTextSharp.text;
using Net.FairfieldTek.Hocr.Enums;

namespace Net.FairfieldTek.Hocr.Pdf
{
    public class PdfCompressorSettings
    {
        public PdfCompressorSettings()
        {
            Dpi = 400;
            ImageType = PdfImageType.Png;
            ImageQuality = 30;
            WriteTextMode = WriteTextMode.Word;
            Language = "eng";
        }


        public int Dpi { get; set; }

        /// <summary>
        ///     Name of the installed font file that you want to use and embed in the pdf, Ex. "ARIALUNI.TTF"
        /// </summary>
        public string FontName { get; set; }

        public long ImageQuality { get; set; }

        public PdfImageType ImageType { get; set; }

        //public string Keywords { get; set; }
        public string Language { get; set; }

        public Rectangle PdfPageSize { get; set; }

        /// <summary>
        ///     write unlerlay text by lin e or word. by line creates smalled pdf files. word is ignored if OcrMode == cuneiform
        /// </summary>
        public WriteTextMode WriteTextMode { get; set; }

        public bool CompressFinalPdf { get; set; } = true;

        public PdfCompatibilityLevel PdfCompatibilityLevel { get; set; } = PdfCompatibilityLevel.Acrobat_7_1_6;

        public dPdfSettings DistillerMode { get; set; } = dPdfSettings.printer;

        public string DistillerOptions { get; set; } = string.Empty;
    }
}