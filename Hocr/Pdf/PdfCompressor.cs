using Hocr.Enums;
using Hocr.ImageProcessors;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hocr.Pdf
{
    public delegate void CompressorExceptionOccurred(PdfCompressor c, Exception x);

    public delegate void CompressorEvent(string msg);

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

            OnCompressorEvent?.Invoke(sessionName + " Creating PDF Reader");
            PdfReader reader = new PdfReader(inputFileName, GhostScriptPath);

            OnCompressorEvent?.Invoke(sessionName + " Creating PDF Writer");
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
                    OnCompressorEvent?.Invoke(sessionName + " Processing page " + i + " of " + reader.PageCount);
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
                    //Console.WriteLine(x.Message);
                    OnCompressorEvent?.Invoke(sessionName + " Image not supported in " + Path.GetFileName(inputFileName) + ". Skipping");
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
        public event CompressorEvent OnCompressorEvent;

        /// <summary>
        ///     PreProcess the image before ocr and converting. Useful for deskewing, etc..
        /// </summary>
        public event PreProcessImage OnPreProcessImage;


        public Tuple<byte[], string> CreateSearchablePdf(byte[] fileData, PdfMeta metaData)
        {
            try
            {
                //string sessionName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

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

                //Convert the PDF 
                //{
                //    GhostScript gs = new GhostScript(GhostScriptPath);
                //    inputDataFilePath = gs.CompressPdf(inputDataFilePath, sessionName, PdfCompatibilityLevel.Acrobat_7_1_7);
                //}
                //OnCompressorEvent?.Invoke(sessionName + " Ran file through ghostscript");

                OnCompressorEvent?.Invoke(sessionName + " Begin Compress and Ocr");

                string pageBody = CompressAndOcr(sessionName, inputDataFilePath, outputDataFilePath, metaData);

                string outputFileName = outputDataFilePath;

                if (PdfSettings.CompressFinalPdf)
                {
                    OnCompressorEvent?.Invoke(sessionName + " Compressing output");
                    GhostScript gs = new GhostScript(GhostScriptPath);
                    outputFileName = gs.CompressPdf(outputDataFilePath, sessionName, PdfSettings.PdfCompatibilityLevel);
                }


                byte[] outFile = File.ReadAllBytes(outputFileName);
                OnCompressorEvent?.Invoke(sessionName + " Destroying session");
                TempData.Instance.DestroySession(sessionName);
                return new Tuple<byte[], string>(outFile, pageBody);
            }
            catch (Exception e)
            {
                OnExceptionOccurred?.Invoke(this, e);
                //Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Tuple<byte[], string>> CreateSearchablePdfAsync(byte[] fileData, PdfMeta metaData)
        {

            string sessionName = TempData.Instance.CreateNewSession();

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
    }
}