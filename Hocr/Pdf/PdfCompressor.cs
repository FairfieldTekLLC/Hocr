using System;
using System.Drawing;
using System.IO;
using iTextSharp.text.io;
using Net.FairfieldTek.Hocr.Enums;
using Net.FairfieldTek.Hocr.Exceptions;
using Net.FairfieldTek.Hocr.ImageProcessors;
using Rectangle = iTextSharp.text.Rectangle;

namespace Net.FairfieldTek.Hocr.Pdf
{
    public delegate void CompressorExceptionOccurred(PdfCompressor c, Exception x);

    public delegate void CompressorEvent(string msg);

    public delegate string PreProcessImage(string bitmapPath);

    


    public class PdfCompressor
    {
        private string GhostScriptPath { get; set; } = string.Empty;
        public PdfCompressor( string ghostScriptPath,PdfCompressorSettings settings = null)
        {
            PdfSettings = settings ?? new PdfCompressorSettings();
            GhostScriptPath = ghostScriptPath;
        }
        
        public PdfCompressorSettings PdfSettings { get; }

        private string CompressAndOcr(string sessionName, string inputFileName, string outputFileName, PdfMeta meta)
        {
            string pageBody = "";

            OnCompressorEvent?.Invoke(sessionName + " Creating PDF Reader");
            PdfReader reader = new PdfReader(inputFileName,  PdfSettings.Dpi,GhostScriptPath);

            OnCompressorEvent?.Invoke(sessionName + " Creating PDF Writer");
            PdfCreator writer =
                new PdfCreator(PdfSettings, outputFileName,  meta, PdfSettings.Dpi) {PdfSettings = {WriteTextMode = PdfSettings.WriteTextMode}};

            try
            {
                for (int i = 1; i <= reader.PageCount; i++)
                {
                    OnCompressorEvent?.Invoke(sessionName + " Processing page " + i + " of " + reader.PageCount);
                    string img = reader.GetPageImage(i, sessionName, this);
                    if (OnPreProcessImage != null) img = OnPreProcessImage(img);
                    Bitmap chk = new Bitmap(img);
                    Rectangle pageSize = new Rectangle(0, 0, chk.Width, chk.Height);
                    chk.Dispose();
                    pageBody = pageBody + writer.AddPage(img, PdfMode.Ocr, sessionName, pageSize);
                }

                writer.SaveAndClose();
                writer.Dispose();
                reader.Dispose();
                return pageBody;
            }
            catch (Exception x)
            {
                OnCompressorEvent?.Invoke(sessionName + " Image not supported in " + Path.GetFileName(inputFileName) + ". Skipping");
                writer.SaveAndClose();
                writer.Dispose();
                reader.Dispose();
                OnExceptionOccurred?.Invoke(this, x);
                throw;
            }
        }

        public event CompressorExceptionOccurred OnExceptionOccurred;
        public event CompressorEvent OnCompressorEvent;
        public event PreProcessImage OnPreProcessImage;

        private int GetPages(byte[] data)
        {
            try
            {
                using (iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(data))
                    return pdfReader.NumberOfPages;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public Tuple<byte[], string> CreateSearchablePdf(byte[] fileData, PdfMeta metaData)
        {
            try
            {
                int PageCountStart = GetPages(fileData);
                string sessionName = TempData.Instance.CreateNewSession();
                OnCompressorEvent?.Invoke("Created Session:" + sessionName);
                string inputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");
                string outputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");
                if (fileData == null || fileData.Length == 0)
                    throw new Exception("No Data in fileData");
                using (FileStream writer = new FileStream(inputDataFilePath, FileMode.Create, FileAccess.Write))
                {
                    writer.Write(fileData, 0, fileData.Length);
                    writer.Flush(true);
                }

                OnCompressorEvent?.Invoke(sessionName + " Wrote binary to file");
                OnCompressorEvent?.Invoke(sessionName + " Begin Compress and Ocr");
                string pageBody = CompressAndOcr(sessionName, inputDataFilePath, outputDataFilePath, metaData);
                string outputFileName = outputDataFilePath;
                if (PdfSettings.CompressFinalPdf)
                {
                    OnCompressorEvent?.Invoke(sessionName + " Compressing output");
                    GhostScript gs = new GhostScript(GhostScriptPath, PdfSettings.Dpi);
                    outputFileName = gs.CompressPdf(outputDataFilePath, sessionName, PdfSettings.PdfCompatibilityLevel, PdfSettings.DistillerMode,
                        PdfSettings.DistillerOptions);
                }

                byte[] outFile = File.ReadAllBytes(outputFileName);
                int PageCountEnd = GetPages(outFile);
                OnCompressorEvent?.Invoke(sessionName + " Destroying session");
                TempData.Instance.DestroySession(sessionName);

                if (PageCountEnd != PageCountStart)
                    throw new PageCountMismatchException("Page count is different", PageCountStart, PageCountEnd);

                return new Tuple<byte[], string>(outFile, pageBody);
            }
            catch (Exception e)
            {
                OnExceptionOccurred?.Invoke(this, e);
                throw new FailedToGenerateException("Error in: CreateSearchablePdf", e);
            }
        }
    }
}