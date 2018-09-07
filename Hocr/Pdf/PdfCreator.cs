using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Hocr.HocrElements;
using Hocr.ImageProcessors;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
namespace Hocr.Pdf
{
    public delegate Image ProcessImageForDisplay(System.Drawing.Image image);

    public delegate Bitmap ProcessImageForOcr(System.Drawing.Image image);

    internal class PdfCreator : IDisposable
    {
        private Document doc;
        private HDocument hDoc;

        private PdfOutline outline;
        private PdfWriter writer;

        private OcrController OcrController;

        public PdfCreator(string newPdf,string tesseractPath)
        {
            OcrController = new OcrController(tesseractPath);
            PDFSettings = new PDFSettings();
            PDFFilePath = newPdf;
            SetupDocumentWriter(newPdf);
            hDoc = new HDocument();
            
        }

        public PdfCreator(string newPdf, string hocrFilePath,string sessionName, string tesseractPath)
        {
            OcrController = new OcrController(tesseractPath);
            PDFSettings = new PDFSettings();
            PDFFilePath = newPdf;
            SetupDocumentWriter(newPdf);
            hDoc = new HDocument();

            AddHocrFile(hocrFilePath,sessionName);
        }

        public PdfCreator(PDFSettings settings, string newPdf, string tesseractPath)
        {
            OcrController = new OcrController(tesseractPath);
            PDFSettings = settings;
            PDFFilePath = newPdf;
            SetupDocumentWriter(newPdf);
            hDoc = new HDocument();
        }

        public string PDFFilePath { get; }
        public PDFSettings PDFSettings { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                doc.Dispose();
                writer.Dispose();

                doc = null;
                writer = null;
                GC.Collect();
            }
            catch
            {
            }
        }

        #endregion

        public void AddHocrFile(string hocrFilePath,string sessionName)
        {
            HDocument doc = new HDocument();
            doc.AddFile(hocrFilePath);

            foreach (HPage p in doc.Pages)
            {
                Stream s = File.OpenRead(p.ImageFile);
                System.Drawing.Image image = System.Drawing.Image.FromStream(s);
                Guid objGuid = image.FrameDimensionsList[0];
                FrameDimension frameDim = new FrameDimension(objGuid);
                image.SelectActiveFrame(frameDim, p.ImageFrameNumber);
                System.Drawing.Image img = ImageProcessor.GetAsBitmap(image, PDFSettings.Dpi);
                AddPage(p, img,sessionName);
            }
        }

        /// <summary>
        ///     If adding an image directly, don't forget to call CreatePage
        /// </summary>
        /// <param name="image"></param>
        private void AddImage(System.Drawing.Image image,string sessionName)
        {
            try
            {
                if (OnProcessImageForDisplay != null)
                {
                    AddImage(OnProcessImageForDisplay(image));
                    return;
                }

                Bitmap bmp = ImageProcessor.GetAsBitmap(image, PDFSettings.Dpi);
                Image i = GetImageForPDF(bmp,sessionName);
                AddImage(i);
                //  i.SetAbsolutePosition(0, 0);
                // doc.SetPageSize(new iTextSharp.text.Rectangle(i.Width, i.Height));
                // i.ScaleAbsolute(doc.PageSize.Width, doc.PageSize.Height);
                // doc.Add(i);
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.Message);
                throw x;
            }
        }

        private void AddImage(Image image)
        {
            try
            {
                //Getting Width of the image width adding the page right & left margin
                float width = image.Width / PDFSettings.Dpi * 72;

                //Getting Height of the image height adding the page top & bottom margin
                float height = image.Height / PDFSettings.Dpi * 72;

                //Creating pdf rectangle with the specified height & width for page size declaration
                Rectangle r = new Rectangle(width, height);

                /*you __MUST__ call SetPageSize() __BEFORE__ calling NewPage()
                * AND __BEFORE__ adding the image to the document
                */

                //Changing the page size of the pdf document based on the rectangle defined
                if (PDFSettings.PdfPageSize == null)
                    doc.SetPageSize(r);
                else
                    doc.SetPageSize(PDFSettings.PdfPageSize);

                image.SetAbsolutePosition(0, 0);
                image.ScaleAbsolute(doc.PageSize.Width, doc.PageSize.Height);
                doc.NewPage();
                doc.Add(image);
                GC.Collect();
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.Message);
                throw x;
            }
        }

        private void AddImage(string imagePath,string sessionName)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
            AddImage(image,sessionName);
        }

        public void AddPage(HPage page, System.Drawing.Image pageImage,string sessionName)
        {
            // doc.NewPage();
            AddImage(pageImage,sessionName);
            WriteUnderlayContent(page);
        }

        public void AddPage(string ImagePath, PdfMode mode,string sessionName)
        {
            AddPage(System.Drawing.Image.FromFile(ImagePath), mode,sessionName);
        }

        public void AddPage(System.Drawing.Image image, PdfMode Mode,string sessionName)
        {
            Guid objGuid = image.FrameDimensionsList[0];
            FrameDimension frameDim = new FrameDimension(objGuid);
            int frameCount = image.GetFrameCount(frameDim);
            for (int i = 0; i < frameCount; i++)
            {

                Bitmap img;

                image.SelectActiveFrame(frameDim, i);

                if (image is Bitmap == false)
                    img = ImageProcessor.GetAsBitmap(image, PDFSettings.Dpi);
                else
                    img = image as Bitmap;

                img.SetResolution(PDFSettings.Dpi, PDFSettings.Dpi);

                if (Mode == PdfMode.ImageOnly)
                    AddImage(image,sessionName);
                if (Mode == PdfMode.Ocr)
                    try
                    {
                        AddImage(image,sessionName);

                        if (OnProcessImageForOcr != null)
                            img = OnProcessImageForOcr(img);
                        OcrController.AddToDocument( PDFSettings.Language, image, ref hDoc,sessionName);
                        HPage page = hDoc.Pages[hDoc.Pages.Count - 1];
                        WriteUnderlayContent(page);
                    }
                    catch (Exception x)
                    {
                        string message = x.Message;
                    }
                if (Mode == PdfMode.TextOnly)
                    try
                    {
                        doc.NewPage();
                        OcrController.AddToDocument(PDFSettings.Language, image, ref hDoc,sessionName);
                        HPage page = hDoc.Pages[hDoc.Pages.Count - 1];
                        WriteDirectContent(page);
                    }
                    catch (Exception)
                    {
                    }

                if (Mode == PdfMode.DrawBlocks)
                    try
                    {
                        OcrController.AddToDocument( PDFSettings.Language, image, ref hDoc,sessionName);
                        HPage page = hDoc.Pages[hDoc.Pages.Count - 1];
                        WritePageDrawBlocks(image, page,sessionName);
                    }
                    catch (Exception)
                    {
                    }

                if (Mode == PdfMode.Debug)
                    try
                    {
                        OcrController.AddToDocument( PDFSettings.Language, image, ref hDoc,sessionName);
                        HPage page = hDoc.Pages[hDoc.Pages.Count - 1];
                        WritePageDrawBlocks(image, page,sessionName);
                        WriteDirectContent(page);
                    }
                    catch (Exception)
                    {
                    }

                img.Dispose();
            }
        }

        public void AddPage(System.Drawing.Image image,string sessionName)
        {
            Guid objGuid = image.FrameDimensionsList[0];
            FrameDimension frameDim = new FrameDimension(objGuid);
            int frameCount = 0;
            try
            {
                frameCount = image.GetFrameCount(frameDim);
            }
            catch (Exception x)
            {
                Bitmap img;
                if (image is Bitmap == false)
                    img = ImageProcessor.GetAsBitmap(image,
                        PDFSettings.Dpi); // AForge.Imaging.Image.Clone((Bitmap)image, PixelFormat.Format24bppRgb);
                else
                    img = image as Bitmap;
                img.SetResolution(PDFSettings.Dpi, PDFSettings.Dpi);

                AddImage(img,sessionName);
            }
            for (int i = 0; i < frameCount; i++)
            {
                //doc.NewPage();

                Bitmap img;

                image.SelectActiveFrame(frameDim, i);

                if (image is Bitmap == false)
                    img = ImageProcessor.GetAsBitmap(image, PDFSettings.Dpi);
                else
                    img = image as Bitmap;

                img.SetResolution(PDFSettings.Dpi, PDFSettings.Dpi);
                AddImage(image,sessionName);
                img.Dispose();
            }
        }

        public void AddPdf(string PdfFile, string BookMarkDesc, string ID)
        {
            if (!File.Exists(PdfFile))
                return;

            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(PdfFile);

            PdfContentByte cb = writer.DirectContent;
            PdfOutline root = cb.RootOutline;

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                if (i > reader.NumberOfPages)
                    break;

                PdfImportedPage page = writer.GetImportedPage(reader, i);
                doc.SetPageSize(reader.GetPageSize(i));
                doc.NewPage();

                int rot = reader.GetPageRotation(i);

                if (rot == 90 || rot == 270)
                    cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                else
                    cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0);

                if (i == 1)
                {
                    doc.Add(new Chunk(BookMarkDesc).SetLocalDestination(ID));
                    outline = new PdfOutline(root, PdfAction.GotoLocalPage(ID, false), BookMarkDesc);
                }
            }
            reader.Close();
        }

        public void AddPdf(string PdfFile)
        {
            if (!File.Exists(PdfFile))
                return;

            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(PdfFile);

            PdfContentByte cb = writer.DirectContent;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                if (i > reader.NumberOfPages)
                    break;

                doc.NewPage();
                PdfImportedPage page = writer.GetImportedPage(reader, i);

                int rot = reader.GetPageRotation(i);

                if (rot == 90 || rot == 270)
                    cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                else
                    cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0);
            }
            reader.Close();
        }

        public void AddPdf(byte[] PdfPage)
        {
            iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(PdfPage);

            PdfContentByte cb = writer.DirectContent;
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                if (i > reader.NumberOfPages)
                    break;

                //set the current page size using the source page
                doc.SetPageSize(reader.GetPageSize(i));
                doc.NewPage();

                PdfImportedPage page = writer.GetImportedPage(reader, i);

                int rot = reader.GetPageRotation(i);

                if (rot == 90 || rot == 270)
                    cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                else
                    cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0);
            }
            reader.Close();
        }

        private Image GetImageForPDF(Bitmap image,string sessionName)
        {
            Image i = null;

            switch (PDFSettings.ImageType)
            {
                case PdfImageType.Tif:
                    i = Image.GetInstance(ImageProcessor.ConvertToCcittFaxTiff(image), ImageFormat.Tiff);
                    break;
                case PdfImageType.Png:
                    i = Image.GetInstance(ImageProcessor.ConvertToImage(image, "PNG", PDFSettings.ImageQuality, PDFSettings.Dpi),
                        ImageFormat.Png);
                    break;
                case PdfImageType.Jpg:
                    i = Image.GetInstance(ImageProcessor.ConvertToImage(image, "JPEG", PDFSettings.ImageQuality, PDFSettings.Dpi),
                        ImageFormat.Jpeg);
                    break;
                case PdfImageType.Bmp:
                    i = Image.GetInstance(ImageProcessor.ConvertToImage(image, "BMP", PDFSettings.ImageQuality, PDFSettings.Dpi),
                        ImageFormat.Bmp);
                    break;
                case PdfImageType.JBig2:
                    JBig2 jbig = new JBig2();
                    i = jbig.ProcessImage(image, sessionName);
                    ;
                    break;
            }
            return i;
        }

        public event ProcessImageForDisplay OnProcessImageForDisplay;
        public event ProcessImageForOcr OnProcessImageForOcr;

        public void SaveAndClose()
        {
            try
            {
                if (doc.PageNumber == 0)
                    doc.NewPage();

                writer.CompressionLevel = 100;
                writer.SetFullCompression();

                doc.Close();
            }
            catch (Exception)
            {
            }
        }

        private void SetupDocumentWriter(string fileName)
        {
            doc = new Document();

            doc.SetMargins(0, 0, 0, 0);

            try
            {
                writer = PdfWriter.GetInstance(doc, new FileStream(fileName, FileMode.Create));
            }
            catch (Exception err)
            {
                //Throw away.
            }
            

            writer.SetMargins(0, 0, 0, 0);
            doc.Open();

            if (PDFSettings == null)
                return;
            doc.AddAuthor(PDFSettings.Author);
            doc.AddTitle(PDFSettings.Title);
            doc.AddSubject(PDFSettings.Subject);
            doc.AddKeywords(PDFSettings.Keywords);
        }

        private void WriteDirectContent(HPage page)
        {
            string pageText = page.Text;
            List<HLine> allLines = new List<HLine>();

            foreach (HParagraph para in page.Paragraphs)
            foreach (HLine line in para.Lines)
                allLines.Add(line);
            foreach (HParagraph para in page.Paragraphs)
            foreach (HLine line in para.Lines)
            {
                line.CleanText();
                if (line.Text.Trim() == string.Empty)
                    continue;

                BBox b = BBox.ConvertBBoxToPoints(line.BBox, PDFSettings.Dpi);

                if (b.Height > 28)
                    continue;

                PdfContentByte cb = writer.DirectContent;

                BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                Font font = new Font(base_font);
                if (PDFSettings.FontName != null && PDFSettings.FontName != string.Empty)
                {
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), PDFSettings.FontName);
                    base_font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    // BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                    font = new Font(base_font);
                }

                float h = 9;

                float font_size = allLines.Select(x => x.BBox.Height).Average() / PDFSettings.Dpi * 72.0f; // Math.Ceiling(b.Height);

                if (font_size == 0)
                    font_size = 2;

                cb.BeginText();
                cb.SetFontAndSize(base_font, (int) Math.Floor(font_size) - 1);
                cb.SetTextMatrix(b.Left, doc.PageSize.Height - b.Top - b.Height);
                cb.ShowText(line.Text);
                cb.EndText();
            }
        }

        private void WritePageDrawBlocks(System.Drawing.Image img, HPage page,string sessionName)
        {
            System.Drawing.Image himage = img;

            Pen bpen = null;
            Pen gpen = null;
            Pen rpen = null;
            Graphics bg = null;
            Bitmap rect_canvas = null;

            rect_canvas = new Bitmap(himage.Width, himage.Height);
            Graphics grPhoto = Graphics.FromImage(rect_canvas);
            grPhoto.DrawImage(himage, new System.Drawing.Rectangle(0, 0, rect_canvas.Width, rect_canvas.Height), 0, 0, rect_canvas.Width, rect_canvas.Height,
                GraphicsUnit.Pixel);
            bg = Graphics.FromImage(rect_canvas);
            bpen = new Pen(Color.Red, 3);
            rpen = new Pen(Color.Blue, 3);
            gpen = new Pen(Color.Green, 3);
            Pen ppen = new Pen(Color.HotPink, 3);
            //dpiX = (int)rect_canvas.HorizontalResolution;
            //dpiY = (int)rect_canvas.VerticalResolution;

            foreach (HParagraph para in page.Paragraphs)
            {
                bg.DrawRectangle(gpen,
                    new System.Drawing.Rectangle(new Point((int) para.BBox.Left, (int) para.BBox.Top),
                        new Size((int) para.BBox.Width, (int) para.BBox.Height)));

                foreach (HLine line in para.Lines)
                {
                    foreach (HWord word in line.Words)
                        bg.DrawRectangle(rpen,
                            new System.Drawing.Rectangle(new Point((int) word.BBox.Left, (int) word.BBox.Top),
                                new Size((int) word.BBox.Width, (int) word.BBox.Height)));
                    bg.DrawRectangle(bpen,
                        new System.Drawing.Rectangle(new Point((int) line.BBox.Left, (int) line.BBox.Top),
                            new Size((int) line.BBox.Width, (int) line.BBox.Height)));
                }
            }
            IList<HLine> combinedLines = page.CombineSameRowLines();
            foreach (HLine l in combinedLines.Where(x => x.LineWasCombined))
                bg.DrawRectangle(ppen,
                    new System.Drawing.Rectangle(new Point((int) l.BBox.Left, (int) l.BBox.Top), new Size((int) l.BBox.Width, (int) l.BBox.Height)));

            AddImage(rect_canvas,sessionName);
        }

        public void WriteUnderlayContent(IList<HOcrClass> Locations)
        {
            BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
            Font font = new Font(base_font);
            if (PDFSettings.FontName != null && PDFSettings.FontName != string.Empty)
            {
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), PDFSettings.FontName);
                base_font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                // BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                font = new Font(base_font);
            }

            foreach (HOcrClass c in Locations)
            {
                BBox b = c.BBox;

                PdfContentByte cb = writer.DirectContentUnder;

                cb.BeginText();
                cb.SetFontAndSize(base_font, c.BBox.Height > 0 ? c.BBox.Height : 2);
                if (b.Format == UnitFormat.Point)
                    cb.SetTextMatrix(b.Left, b.Top - b.Height + 2);
                else
                    cb.SetTextMatrix(b.Left, doc.PageSize.Height - b.Top - b.Height + 2);

                cb.ShowText(c.Text.Trim());
                cb.EndText();
            }
        }

        private void WriteUnderlayContent(HPage page)
        {
            string pageText = page.Text;
            foreach (HParagraph para in page.Paragraphs)
            foreach (HLine line in para.Lines)
            {
                if (PDFSettings.WriteTextMode == WriteTextMode.Word)
                {
                    line.AlignTops();

                    foreach (HWord c in line.Words)
                    {
                        c.CleanText();
                        BBox b = BBox.ConvertBBoxToPoints(c.BBox, PDFSettings.Dpi);

                        if (b.Height > 28)
                            continue;
                        PdfContentByte cb = writer.DirectContentUnder;

                        BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                        Font font = new Font(base_font);
                        if (PDFSettings.FontName != null && PDFSettings.FontName != string.Empty)
                        {
                            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), PDFSettings.FontName);
                            base_font = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            // BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                            font = new Font(base_font);
                        }

                        cb.BeginText();
                        cb.SetFontAndSize(base_font, b.Height > 0 ? b.Height : 2);
                        cb.SetTextMatrix(b.Left, doc.PageSize.Height - b.Top - b.Height + 2);
                        cb.SetWordSpacing(DocWriter.SPACE);
                        cb.ShowText(c.Text.Trim() + " ");
                        cb.EndText();
                    }
                }

                if (PDFSettings.WriteTextMode == WriteTextMode.Line)
                {
                    line.CleanText();
                    BBox b = BBox.ConvertBBoxToPoints(line.BBox, PDFSettings.Dpi);

                    if (b.Height > 28)
                        continue;

                    BBox lineBox = BBox.ConvertBBoxToPoints(line.BBox, PDFSettings.Dpi);
                    PdfContentByte cb = cb = writer.DirectContentUnder;

                    BaseFont base_font = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.WINANSI, false);
                    Font font = new Font(base_font);

                    cb.BeginText();
                    cb.SetFontAndSize(base_font, b.Height > 0 ? b.Height : 2);
                    cb.SetTextMatrix(b.Left, doc.PageSize.Height - b.Top - b.Height + 2);
                    cb.SetWordSpacing(.25f);
                    cb.ShowText(line.Text);
                    cb.EndText();
                }

                if (PDFSettings.WriteTextMode == WriteTextMode.Character)
                {
                    line.AlignTops();

                    foreach (HWord word in line.Words)
                    {
                        word.AlignCharacters();
                        foreach (HChar c in word.Characters)
                        {
                            BBox b = BBox.ConvertBBoxToPoints(c.BBox, PDFSettings.Dpi);
                            BBox lineBox = BBox.ConvertBBoxToPoints(c.BBox, PDFSettings.Dpi);
                            PdfContentByte cb = cb = writer.DirectContentUnder;

                            BaseFont base_font = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.WINANSI, false);
                            Font font = new Font(base_font);

                            cb.BeginText();
                            cb.SetFontAndSize(base_font, b.Height > 0 ? b.Height : 2);

                            cb.SetTextMatrix(b.Left, doc.PageSize.Height - b.Top - b.Height + 2);
                            cb.SetCharacterSpacing(-1f);
                            cb.ShowText(c.Text.Trim());
                            cb.EndText();
                        }
                    }
                }
            }
        }
    }
}