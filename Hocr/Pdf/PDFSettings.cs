using iTextSharp.text;

namespace Hocr.Pdf
{
    public  enum PdfMode
    {
        Ocr,
        DrawBlocks,
        TextOnly,
        ImageOnly,
        Debug
    }

    public enum WriteTextMode
    {
        Line,
        Word,
        Character
    }

    public enum PdfImageType
    {
        Bmp,
        Jpg,
        Tif,
        Png,
        JBig2,
        Gif
    }

    
    public  class PDFSettings
    {
        public PDFSettings()
        {
            Dpi = 600;
    
            ImageType = PdfImageType.Png;
    
            ImageQuality = 30;
            WriteTextMode = WriteTextMode.Word;
    
            Language = "eng";
        }

        public string Author { get; set; }
        public int Dpi { get; set; }

        /// <summary>
        ///     Name of the installed font file that you want to use and embed in the pdf, Ex. "ARIALUNI.TTF"
        /// </summary>
        public string FontName { get; set; }

        public long ImageQuality { get; set; }
        public PdfImageType ImageType { get; set; }
        public string Keywords { get; set; }
        public string Language { get; set; }
        public Rectangle PdfPageSize { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }

        /// <summary>
        ///     write unlerlay text by lin e or word. by line creates smalled pdf files. word is ignored if OcrMode == cuneiform
        /// </summary>
        public WriteTextMode WriteTextMode { get; set; }
    }
}