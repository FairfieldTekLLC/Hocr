using System;
using System.Text;

namespace Net.FairfieldTek.Hocr.HocrElements
{
    internal class BBox
    {
        private readonly float _dpi;

        public BBox(float dpi)
        {
            Format = UnitFormat.Pixel;
            _dpi = dpi;
        }

        public BBox(string boxvalues, float dpi)
        {
            _dpi = dpi;
            string[] values = boxvalues.Trim().Split(new char[char.MinValue]);
            if (values.Length < 4)
                return;

            if (int.TryParse(values[0].Trim(), out int testout))
                Left = testout;

            if (int.TryParse(values[1].Trim(), out testout))
                Top = testout;

            if (int.TryParse(values[2].Trim(), out testout))
                Width = testout;

            if (int.TryParse(values[3].Trim(), out testout))
                Height = testout;

            //accurate width
            Width = Width - Left;
            //accurate height
            Height = Height - Top;

            Format = UnitFormat.Pixel;
        }

        public float CenterLine => Top + Height / 2;

        public BBox DefaultPointBBox => ConvertBBoxToPoints(this, _dpi);

        public UnitFormat Format { get; set; }

        public float Height { get; set; }

        public float Left { get; set; }

        public float Top { get; set; }

        public float Width { get; set; }

        public static BBox ConvertBBoxToPoints(BBox bbox, float dpi)
        {
            if (dpi == 0)
                throw new Exception("DPI is zero.");

            BBox newBbox = new BBox(dpi)
            {
                Left = bbox.Left * 72 / dpi,
                Top = bbox.Top * 72 / dpi,
                Width = bbox.Width * 72 / dpi,
                Height = bbox.Height * 72 / dpi,
                Format = UnitFormat.Point
            };
            return newBbox;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Left: " + Left);
            sb.AppendLine("Top: " + Top);
            sb.AppendLine("Width: " + Width);
            sb.AppendLine("Height: " + Height);
            sb.AppendLine("CenterLine: " + CenterLine);
            return sb.ToString();
        }
    }
}