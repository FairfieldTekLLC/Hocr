using System;
using Hocr.ImageProcessors;
using iTextSharp.text.pdf.parser;
using Image = System.Drawing.Image;

namespace Hocr.Pdf
{
    public delegate void SplitPdfForBookmark(string bookmarkTitle, string savedFilePath);

    internal class PdfReader : IDisposable
    {
        private readonly string _ghostScriptPath;

        public PdfReader(string sourcePdf, string ghostScriptPath)
        {
            _ghostScriptPath = ghostScriptPath;
            SourcePdf = sourcePdf;
            TextReader = new iTextSharp.text.pdf.PdfReader(sourcePdf);
        }

        public iTextSharp.text.pdf.PdfReader TextReader {
            get; private set;
        }

        public int PageCount => TextReader.NumberOfPages;

        public string SourcePdf {
            get;
        }


        public void Dispose()
        {
            try
            {
                TextReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            TextReader = null;
        }
        
        public string GetPageImage(int pageNumber, bool useGhostscript, string sessionName)
        {
            if (useGhostscript)
                return GetPageImageWithGhostScript(pageNumber, sessionName);

            PdfReaderContentParser parser = new PdfReaderContentParser(TextReader);
            MyImageRenderListener listener = new MyImageRenderListener { PageNumber = pageNumber, Reader = this };

            try
            {
                parser.ProcessContent(pageNumber, listener);

                if (listener.ParsedImages.Count > 1 && GetPageText(pageNumber).Trim() != string.Empty)
                    return null;

                if (listener.ParsedImages.Count > 0)
                {
                    string s = TempData.Instance.CreateTempFile(sessionName, ".bmp");
                    Image bmp = ImageProcessor.ConvertToImage(listener.ParsedImages[0].Image, "BMP", 100, 300);
                    bmp.Save(s);
                    return s;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Getting page via GhostScript");
                return GetPageImageWithGhostScript(pageNumber, sessionName);
            }
            return null;
        }

        private string GetPageImageWithGhostScript(int pageNumber, string sessionName)
        {
            GhostScript g = new GhostScript(_ghostScriptPath);
            string imgFile = g.ConvertPdfToBitmap(SourcePdf, pageNumber, pageNumber, sessionName);
            return imgFile;
        }

        public string GetPageText(int pageNumber)
        {
            PdfTextExtractionStrategy strat = new PdfTextExtractionStrategy();
            string te = PdfTextExtractor.GetTextFromPage(TextReader, pageNumber, strat);

            return te;
        }
        
    }






}