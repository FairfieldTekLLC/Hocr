using System;
using System.IO;
using System.Threading.Tasks;
using Hocr.Enums;
using Hocr.ImageProcessors;

namespace Hocr.Pdf
{
    public delegate void CompressorExceptionOccurred(PdfCompressor c, Exception x);

    public delegate string PreProcessImage(string bitmapPath);

    public class PdfCompressor
    {
        public PdfCompressor(string ghostScriptPath, string tesseractPath, PdfCompressorSettings settings = null)
        {
            TesseractPath = tesseractPath;
            GhostScriptPath = ghostScriptPath;
            PdfSettings = settings ?? new PdfCompressorSettings();
        }

        private string GhostScriptPath { get; }

        private string TesseractPath { get; }


        public PdfCompressorSettings PdfSettings { get; }

        private string CompressAndOcr(string sessionName, string inputFileName, string outputFileName, PdfMeta meta)
        {
            string pageBody = "";
            PdfReader reader = new PdfReader(inputFileName, GhostScriptPath);
            PdfCreator writer = new PdfCreator(PdfSettings, outputFileName, TesseractPath, meta)
            {
                PdfSettings =
                {
                    WriteTextMode = WriteTextMode.Word
                }
            };

            try
            {
                for (int i = 1; i <= reader.PageCount; i++)
                {
                    string img = reader.GetPageImage(i, true, sessionName);
                    if (OnPreProcessImage != null)
                        img = OnPreProcessImage(img);
                    pageBody = pageBody + writer.AddPage(img, PdfMode.Ocr, sessionName);
                }

                writer.SaveAndClose();
                writer.Dispose();
                reader.Dispose();
                return pageBody;
            }
            catch (Exception x)
            {
                try
                {
                    Console.WriteLine(x.Message);
                    Console.WriteLine("Image not supported in " + Path.GetFileName(inputFileName) + ". Skipping");
                    writer.SaveAndClose();
                    writer.Dispose();
                    reader.Dispose();
                }
                catch (Exception)
                {
                }

                OnExceptionOccurred?.Invoke(this, x);
            }

            return "";
        }

        public event CompressorExceptionOccurred OnExceptionOccurred;

        /// <summary>
        ///     PreProcess the image before ocr and converting. Useful for deskewing, etc..
        /// </summary>
        public event PreProcessImage OnPreProcessImage;


        public Tuple<byte[], string> CreateSearchablePdf(byte[] fileData, PdfMeta metaData)
        {
            try
            {
                string sessionName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

                TempData.Instance.CreateNewSession(sessionName);

                string inputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");
                string outputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");

                using (FileStream writer = new FileStream(inputDataFilePath, FileMode.Create, FileAccess.Write))
                {
                    writer.Write(fileData, 0, fileData.Length);
                    writer.Flush(true);
                }

                //Convert the PDF 
                {
                    GhostScript gs = new GhostScript(GhostScriptPath);
                    inputDataFilePath = gs.CompressPdf(inputDataFilePath, sessionName, PdfCompatibilityLevel.Acrobat_7_1_7);
                }


                string pageBody = CompressAndOcr(sessionName, inputDataFilePath, outputDataFilePath, metaData);

                string outputFileName = outputDataFilePath;

                if (PdfSettings.CompressFinalPdf)
                {
                    GhostScript gs = new GhostScript(GhostScriptPath);
                    outputFileName = gs.CompressPdf(outputDataFilePath, sessionName, PdfSettings.PdfCompatibilityLevel);
                }


                byte[] outFile = File.ReadAllBytes(outputFileName);
                TempData.Instance.DestroySession(sessionName);
                return new Tuple<byte[], string>(outFile, pageBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Tuple<byte[], string>> CreateSearchablePdfAsync(byte[] fileData, PdfMeta metaData)
        {
            try
            {
                string sessionName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

                TempData.Instance.CreateNewSession(sessionName);

                string inputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");
                string outputDataFilePath = TempData.Instance.CreateTempFile(sessionName, ".pdf");

                using (FileStream writer = new FileStream(inputDataFilePath, FileMode.Create, FileAccess.Write))
                {
                    writer.Write(fileData, 0, fileData.Length);
                    writer.Flush(true);
                }

                string pageBody = await Task.Run(() => CompressAndOcr(sessionName, inputDataFilePath, outputDataFilePath, metaData));

                string outputFileName = outputDataFilePath;

                if (PdfSettings.CompressFinalPdf)
                {
                    GhostScript gs = new GhostScript(GhostScriptPath);
                    outputFileName = gs.CompressPdf(outputDataFilePath, sessionName, PdfSettings.PdfCompatibilityLevel);
                }

                byte[] outFile = File.ReadAllBytes(outputFileName);
                TempData.Instance.DestroySession(sessionName);
                return new Tuple<byte[], string>(outFile, pageBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}