using System.Drawing;
using System.Text;

namespace Hocr.HocrElements
{
 

    internal class BBox
    {
        public BBox()
        {
            Format = UnitFormat.Pixel;
        }

        public BBox(string boxvalues)
        {
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

        public BBox DefaultPointBBox => ConvertBBoxToPoints(this, 300);

        public UnitFormat Format { get; set; }
        public float Height { get; set; }
        public float Left { get; set; }

        public Rectangle Rectangle => new Rectangle(new Point((int) Left, (int) Top), new Size((int) Width, (int) Height));

        public float Top { get; set; }
        public float Width { get; set; }

        public static BBox ConvertBBoxToPoints(BBox bbox, int resolution)
        {
            if (resolution == 0)
                resolution = 300;

            BBox newBbox = new BBox
            {
                Left = bbox.Left * 72 / resolution,
                Top = bbox.Top * 72 / resolution,
                Width = bbox.Width * 72 / resolution,
                Height = bbox.Height * 72 / resolution,
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