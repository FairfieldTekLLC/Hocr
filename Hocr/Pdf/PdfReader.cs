using System;
using Hocr.ImageProcessors;

namespace Hocr.Pdf
{
    internal class PdfReader : IDisposable
    {
        private readonly int _dpi;
        private readonly string _ghostScriptPath;
        public PdfReader(string sourcePdf, string ghostScriptPath, int dpi)
        {
            _ghostScriptPath = ghostScriptPath;
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
            GhostScript g = new GhostScript(_ghostScriptPath, _dpi);
            string imgFile = g.ConvertPdfToBitmap(SourcePdf, pageNumber, pageNumber, sessionName);
            return imgFile;
        }
    }
}