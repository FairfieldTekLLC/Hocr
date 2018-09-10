using System.Collections.Generic;
using System.Drawing;
using System.IO;
using iTextSharp.text.pdf.parser;

namespace Hocr.Pdf
{
    internal class MyImageRenderListener : IRenderListener
    {
        public List<ParsedPageImage> ParsedImages = new List<ParsedPageImage>();
        public int PageNumber {
            get; set;
        }

        public PdfReader Reader {
            get; set;
        }

        public void BeginTextBlock()
        {
        }

        public void EndTextBlock()
        {
        }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            try
            {
                PdfImageObject image = renderInfo.GetImage();
                if (image == null)
                    return;

                if (renderInfo.GetRef() == null)
                    return;

                int num = renderInfo.GetRef().Number;

                byte[] bytes = image.GetImageAsBytes();
                if (bytes == null)
                    return;
                ParsedPageImage pi = new ParsedPageImage {IndirectReferenceNum = num, PdfImageObject = image, Image = image.GetDrawingImage()};
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    pi.Image = Image.FromStream(ms);
                    ParsedImages.Add(pi);
                }
            }
            catch (IOException)
            {
             //
            }
        }

        public void RenderText(TextRenderInfo renderInfo)
        {
        }
    }
}
