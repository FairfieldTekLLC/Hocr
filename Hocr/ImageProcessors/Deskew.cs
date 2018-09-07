using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Hocr.ImageProcessors
{
    internal class Deskew
    {
        // The range of angles to search for lines
        private const double CAlphaStart = -20;

        private const double CAlphaStep = 0.2;

        // The Bitmap
        private readonly Bitmap _cBmp;

        private double[] _cCosA;

        private int _cDCount;

        // Range of d
        private double _cDMin;

        private const double CdStep = 1;

        // Count of points that fit in a line.
        private int[] _cHMatrix;

        // Precalculation of sin and cos.
        private double[] _cSinA;

        private const int CSteps = 40 * 5;

        public Deskew(Bitmap bmp)
        {
            _cBmp = bmp;
        }

        //    ' Hough Transforamtion:
        private void Calc()
        {
            int x;
            int y;
            int hMin = _cBmp.Height / 4;
            int hMax = _cBmp.Height * 3 / 4;
            Init();
            for (y = hMin; y < hMax; y++)
            for (x = 1; x < _cBmp.Width - 2; x++)
                //' Only lower edges are considered.
                if (IsBlack(x, y))
                    if (IsBlack(x, y + 1) == false)
                        Calc(x, y);
        }

        //    ' Calculate all lines through the point (x,y).
        private void Calc(int x, int y)
        {
            for (int alpha = 0; alpha < CSteps - 1; alpha++)
            {
                double d = y * _cCosA[alpha] - x * _cSinA[alpha];
                int dIndex = (int) CalcDIndex(d);
                int index = dIndex * CSteps + alpha;

                try
                {
                    _cHMatrix[index] += 1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private double CalcDIndex(double d)
        {
            return Convert.ToInt32(d - _cDMin);
        }

        public double GetAlpha(int index)
        {
            return CAlphaStart + index * CAlphaStep;
        }

        // calculate the skew angle of the image cBmp

        public double GetSkewAngle()
        {
            int i;
            double sum = 0;
            int count = 0;

            //' Hough Transformation
            Calc();
            //' Top 20 of the detected lines in the image.
            var hl = GetTop(20);
            //' Average angle of the lines
            for (i = 0; i < 19; i++)
            {
                sum += hl[i].Alpha;
                count += 1;
            }
            return sum / count;
        }

        //    ' Calculate the Count lines in the image with most points.
        private HougLine[] GetTop(int count)
        {
            var hl = new HougLine[count];
            for (int i = 0; i < count; i++)
                hl[i] = new HougLine();
            for (int i = 0; i < _cHMatrix.Length - 1; i++)
                if (_cHMatrix[i] > hl[count - 1].Count)
                {
                    hl[count - 1].Count = _cHMatrix[i];
                    hl[count - 1].Index = i;
                    int j = count - 1;
                    while (j > 0 && hl[j].Count > hl[j - 1].Count)
                    {
                        var tmp = hl[j];
                        hl[j] = hl[j - 1];
                        hl[j - 1] = tmp;
                        j -= 1;
                    }
                }
            for (int i = 0; i < count; i++)
            {
                int dIndex = hl[i].Index / CSteps;
                int alphaIndex = hl[i].Index - dIndex * CSteps;
                hl[i].Alpha = GetAlpha(alphaIndex);
                hl[i].D = dIndex + _cDMin;
            }
            return hl;
        }

        private void Init()
        {
            //' Precalculation of sin and cos.
            _cSinA = new double[CSteps - 1];
            _cCosA = new double[CSteps - 1];
            for (int i = 0; i < CSteps - 1; i++)
            {
                double angle = GetAlpha(i) * Math.PI / 180.0;
                _cSinA[i] = Math.Sin(angle);
                _cCosA[i] = Math.Cos(angle);
            }
            //' Range of d
            _cDMin = -_cBmp.Width;
            _cDCount = (int) (2 * (_cBmp.Width + _cBmp.Height) / CdStep);
            _cHMatrix = new int[_cDCount * CSteps];
        }

        private bool IsBlack(int x, int y)
        {
            var c = _cBmp.GetPixel(x, y);
            double luminance = c.R * 0.299 + c.G * 0.587 + c.B * 0.114;
            return luminance < 140;
        }

        public static Bitmap RotateImage(Bitmap bmp, double angle)
        {
            Bitmap tmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            tmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
            var g = Graphics.FromImage(tmp);
            try
            {
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
                g.RotateTransform((float) angle);
                g.DrawImage(bmp, 0, 0);
            }
            finally
            {
                g.Dispose();
            }
            return tmp;
        }

        // Representation of a line in the image.
        public class HougLine
        {
            //' The line is represented as all x,y that solve y*cos(alpha)-x*sin(alpha)=d
            public double Alpha;

            //' Count of points in the line.
            public int Count;

            public double D;

            //' Index in Matrix.
            public int Index;
        }
    }
}