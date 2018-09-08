using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
