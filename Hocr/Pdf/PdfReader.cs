using System;
using Net.FairfieldTek.Hocr.ImageProcessors;

namespace Net.FairfieldTek.Hocr.Pdf
{
    internal class PdfReader : IDisposable
    {
        private readonly int _dpi;

        private string GhostScriptPath { get; set; }

        public PdfReader(string sourcePdf,  int dpi,string ghostscriptPath)
        {
            GhostScriptPath = ghostscriptPath;
            SourcePdf = sourcePdf;
            TextReader = new iTextSharp.text.pdf.PdfReader(sourcePdf);
            _dpi = dpi;
        }

        public iTextSharp.text.pdf.PdfReader TextReader { get; private set; }
        public int PageCount => TextReader.NumberOfPages;
        public string SourcePdf { get; }

        public void Dispose()
        {
            try
            {
                TextReader.Close();
            }
            catch (Exception)
            {
                //Just continue on.
            }

            TextReader = null;
        }

        public string GetPageImage(int pageNumber, string sessionName, PdfCompressor pdfCompressor)
        {
            return GetPageImageWithGhostScript(pageNumber, sessionName);
        }

        private string GetPageImageWithGhostScript(int pageNumber, string sessionName)
        {
            GhostScript g = new GhostScript(GhostScriptPath, _dpi);
            string imgFile = g.ConvertPdfToBitmap(SourcePdf, pageNumber, pageNumber, sessionName);
            return imgFile;
        }
    }
}