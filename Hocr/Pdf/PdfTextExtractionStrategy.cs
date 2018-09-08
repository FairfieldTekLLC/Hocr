using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hocr.HocrElements;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace Hocr.Pdf
{
    internal class PdfTextExtractionStrategy : LocationTextExtractionStrategy
    {
        public PdfTextExtractionStrategy()
        {
            Elements = new List<HOcrClass>();
        }

        public IList<HOcrClass> Elements {
            get;
        }
        
        public override void RenderText(TextRenderInfo info)
        {
            Vector bottomLeft = info.GetDescentLine().GetStartPoint();
            Vector topRight = info.GetAscentLine().GetEndPoint();
            Rectangle rect = new Rectangle(bottomLeft[Vector.I1], bottomLeft[Vector.I2], topRight[Vector.I1], topRight[Vector.I2]);
            BBox b = new BBox {Format = UnitFormat.Point, Height = rect.Height, Left = rect.Left};
            b.Height = rect.Height;
            b.Width = rect.Width;
            b.Top = rect.Top;
            HOcrClass o = new HOcrClass {BBox = b, Text = info.GetText(), ClassName = "xocr_word"};
            Elements.Add(o);
            base.RenderText(info);
        }
    }
}
