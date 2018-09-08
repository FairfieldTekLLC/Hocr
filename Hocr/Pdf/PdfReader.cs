using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hocr.ImageProcessors;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Image = System.Drawing.Image;

namespace Hocr.Pdf
{
    public delegate void SplitPdfForBookmark(string bookmarkTitle, string savedFilePath);

    internal class PdfReader : IDisposable
    {
        private readonly string _ghostScriptPath;

        public PdfReader(iTextSharp.text.pdf.PdfReader r, string ghostScriptPath)
        {
            _ghostScriptPath = ghostScriptPath;
            TextReader = r;
        }

        public PdfReader(string sourcePdf, string ghostScriptPath)
        {
            _ghostScriptPath = ghostScriptPath;
            SourcePdf = sourcePdf;
            TextReader = new iTextSharp.text.pdf.PdfReader(sourcePdf);
        }

        public PdfReader(string sourcePdf, string ownerPassword, string ghostScriptPath)
        {
            _ghostScriptPath = ghostScriptPath;
            SourcePdf = sourcePdf;
            TextReader = new iTextSharp.text.pdf.PdfReader(sourcePdf, Encoding.UTF8.GetBytes(ownerPassword));
        }

        public PdfReader(byte[] sourcePdf, string ghostScriptPath)
        {
            _ghostScriptPath = ghostScriptPath;
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

        public void Close()
        {
            TextReader.Close();
        }

        public void ExtractPages(string outputPdfPath, int startRange, int endRage)
        {
            // create new pdf of pages in the extractPages list
            Document document = new Document();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                document.AddDocListener(writer);
                for (int p = 1; p <= TextReader.NumberOfPages; p++)
                {
                    if (p < startRange || p > endRage)
                        continue;

                    document.SetPageSize(TextReader.GetPageSizeWithRotation(p));
                    document.NewPage();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage pageImport = writer.GetImportedPage(TextReader, p);
                    int rot = TextReader.GetPageRotation(p);
                    if (rot == 90 || rot == 270)
                        cb.AddTemplate(pageImport, 0, -1.0F, 1.0F, 0, 0, TextReader.GetPageSizeWithRotation(p).Height);
                    else
                        cb.AddTemplate(pageImport, 1.0F, 0, 0, 1.0F, 0, 0);
                }
                TextReader.Close();
                document.Close();
                File.WriteAllBytes(outputPdfPath, memoryStream.ToArray());
            }
        }
        public void ExtractPages(string sourcePdfPath, string outputPdfPath, string extractRange, string password)
        {
            if (sourcePdfPath == outputPdfPath)
                throw new Exception("For Extracting PDFs the source and output cannot be the same -- use Delete and inverse your range");

            List<int> pagesToExtract = StringRangeToListInt(extractRange);
            // create new pdf of pages in the extractPages list
            Document document = new Document();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                document.AddDocListener(writer);
                for (int p = 1; p <= TextReader.NumberOfPages; p++)
                {
                    if (pagesToExtract.FindIndex(s => s == p) == -1)
                        continue;
                    document.SetPageSize(TextReader.GetPageSizeWithRotation(p));
                    document.NewPage();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage pageImport = writer.GetImportedPage(TextReader, p);
                    int rot = TextReader.GetPageRotation(p);
                    if (rot == 90 || rot == 270)
                        cb.AddTemplate(pageImport, 0, -1.0F, 1.0F, 0, 0, TextReader.GetPageSizeWithRotation(p).Height);
                    else
                        cb.AddTemplate(pageImport, 1.0F, 0, 0, 1.0F, 0, 0);
                }
                TextReader.Close();
                document.Close();
                File.WriteAllBytes(outputPdfPath, memoryStream.ToArray());
            }
        }

        public byte[] ExtractPagesToPdfBytes(string extractRange, string password)
        {
            List<int> pagesToExtract = StringRangeToListInt(extractRange);
            // create new pdf of pages in the extractPages list
            Document document = new Document();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                document.AddDocListener(writer);
                for (int p = 1; p <= TextReader.NumberOfPages; p++)
                {
                    if (pagesToExtract.FindIndex(s => s == p) == -1)
                        continue;
                    document.SetPageSize(TextReader.GetPageSizeWithRotation(p));
                    document.NewPage();
                    PdfContentByte cb = writer.DirectContent;
                    PdfImportedPage pageImport = writer.GetImportedPage(TextReader, p);
                    int rot = TextReader.GetPageRotation(p);

                    // cb.AddTemplate(pageImport, 1.0F, 0, 0, 1.0F, 0, 0);
                    if (rot == 90 || rot == 270)
                        cb.AddTemplate(pageImport, 0, -1.0F, 1.0F, 0, 0, TextReader.GetPageSizeWithRotation(p).Height);
                    else
                        cb.AddTemplate(pageImport, 1.0F, 0, 0, 1.0F, 0, 0);
                }
                TextReader.Close();
                document.Close();
                return memoryStream.ToArray();
            }
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

       
        private static List<int> StringRangeToListInt(string userRange)
        {
            // parse pagesToExtract string to List of all pages

            List<int> pagesToList = new List<int>();

            // check for non-consecutive ranges :: 1,5,10

            if (userRange.IndexOf(",", StringComparison.Ordinal) != -1)
            {
                string[] tmpHold = userRange.Split(',');

                foreach (string nonconseq in tmpHold)
                    // check for ranges :: 1-5

                    if (nonconseq.IndexOf("-", StringComparison.Ordinal) != -1)
                    {
                        string[] rangeHold = nonconseq.Split('-');

                        for (int i = Convert.ToInt32(rangeHold[0]); i <= Convert.ToInt32(rangeHold[1]); i++)
                            pagesToList.Add(i);
                    }

                    else
                    {
                        pagesToList.Add(Convert.ToInt32(nonconseq));
                    }
            }

            else
            {
                // check for ranges :: 1-5

                if (userRange.IndexOf("-", StringComparison.Ordinal) != -1)
                {
                    string[] rangeHold = userRange.Split('-');

                    for (int i = Convert.ToInt32(rangeHold[0]); i <= Convert.ToInt32(rangeHold[1]); i++)
                        pagesToList.Add(i);
                }

                else
                {
                    // single number found :: 1

                    pagesToList.Add(Convert.ToInt32(userRange));
                }
            }

            return pagesToList;
        }
    }






}