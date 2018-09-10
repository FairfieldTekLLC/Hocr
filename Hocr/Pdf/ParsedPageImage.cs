using System.Drawing;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Hocr.Pdf
{
    internal class ParsedPageImage
    {
        public Image Image {
            get; set;
        }
        public int IndirectReferenceNum {
            get; set;
        }
        public PdfImageObject PdfImageObject {
            get; set;
        }
        public PRStream PrStream {
            get; set;
        }
    }
}
