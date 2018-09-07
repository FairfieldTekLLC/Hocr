using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hocr.HocrElements;


namespace Hocr.ImageProcessors
{
    internal class ImageProcessor
    {
        #region CropUnwantedBackground

        public static Bitmap AutoCrop(Bitmap bmp)
        {
            Color? backColor = GetMatchedBackColor(bmp);
            if (backColor.HasValue)
            {
                Point[] bounds = GetImageBounds(bmp, backColor);
                int diffX = bounds[1].X - bounds[0].X + 1;
                int diffY = bounds[1].Y - bounds[0].Y + 1;
                Bitmap croppedBmp = new Bitmap(diffX, diffY);
                Graphics g = Graphics.FromImage(croppedBmp);
                Rectangle destRect = new Rectangle(0, 0, croppedBmp.Width, croppedBmp.Height);
                Rectangle srcRect = new Rectangle(bounds[0].X, bounds[0].Y, diffX, diffY);
                g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
                return croppedBmp;
            }
            return null;
        }

        #endregion

        public static Bitmap Blur(Bitmap image, int blurSize)
        {
            Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
            {
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            // look at every pixel in the blur rectangle
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
            {
                int avgR = 0, avgG = 0, avgB = 0;
                int blurPixelCount = 0;

                // average the color of the red, green and blue for each pixel in the
                // blur size while making sure you don't go outside the image bounds
                for (int x = xx; x < xx + blurSize && x < image.Width; x++)
                for (int y = yy; y < yy + blurSize && y < image.Height; y++)
                {
                    Color pixel = blurred.GetPixel(x, y);

                    avgR += pixel.R;
                    avgG += pixel.G;
                    avgB += pixel.B;

                    blurPixelCount++;
                }

                avgR = avgR / blurPixelCount;
                avgG = avgG / blurPixelCount;
                avgB = avgB / blurPixelCount;

                // now that we know the average for the blur size, set each pixel to that color
                for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                    blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
            }

            return blurred;
        }

        public static Bitmap ConvertGrayscale(Bitmap image)
        {
            Rectangle rectangle = new Rectangle(0, 0, image.Width, image.Height);
            Bitmap blackAndWhite = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blackAndWhite))
            {
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }

            // for every pixel in the rectangle region
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width && xx < image.Width; xx++)
            for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height && yy < image.Height; yy++)
            {
                // average the red, green and blue of the pixel to get a gray value
                Color pixel = blackAndWhite.GetPixel(xx, yy);
                int avg = (pixel.R + pixel.G + pixel.B) / 3;

                blackAndWhite.SetPixel(xx, yy, Color.FromArgb(0, avg, avg, avg));
            }

            return blackAndWhite;
        }

        public static Bitmap ConvertToBitonal(Bitmap original)
        {
            Bitmap source = null;

            // If original bitmap is not already in 32 BPP, ARGB format, then convert
            if (original.PixelFormat != PixelFormat.Format32bppArgb)
            {
                source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.DrawImageUnscaled(original, 0, 0);
                }
            }
            else
            {
                source = original;
            }

            // Lock source bitmap in memory
            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Copy image data to binary array
            int imageSize = sourceData.Stride * sourceData.Height;
            byte[] sourceBuffer = new byte[imageSize];
            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

            // Unlock source bitmap
            source.UnlockBits(sourceData);

            // Create destination bitmap
            Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);
            destination.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            // destination.SetResolution(200,200);

            // Lock destination bitmap in memory
            BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly,
                PixelFormat.Format1bppIndexed);

            // Create destination buffer
            imageSize = destinationData.Stride * destinationData.Height;
            byte[] destinationBuffer = new byte[imageSize];

            int height = source.Height;
            int width = source.Width;
            int threshold = 580;

            // Iterate lines
            for (int y = 0; y < height; y++)
            {
                int sourceIndex = y * sourceData.Stride;
                int destinationIndex = y * destinationData.Stride;
                byte destinationValue = 0;
                int pixelValue = 128;

                // Iterate pixels
                for (int x = 0; x < width; x++)
                {
                    // Compute pixel brightness (i.e. total of Red, Green, and Blue values) - Thanks murx
                    //                           B                             G                              R
                    int pixelTotal = sourceBuffer[sourceIndex] + sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2];
                    if (pixelTotal > threshold)
                        destinationValue += (byte) pixelValue;
                    if (pixelValue == 1)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                        destinationIndex++;
                        destinationValue = 0;
                        pixelValue = 128;
                    }
                    else
                    {
                        pixelValue >>= 1;
                    }
                    sourceIndex += 4;
                }
                if (pixelValue != 128)
                    destinationBuffer[destinationIndex] = destinationValue;
            }

            // Copy binary image data to destination bitmap
            Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

            // Unlock destination bitmap
            destination.UnlockBits(destinationData);

            // Dispose of source if not originally supplied bitmap
            if (source != original)
                source.Dispose();

            // Return
            return destination;
        }

        //public static Image ConvertToCCITTFaxTiff(string imageToConvert)
        //{
        //    Image img = Image.FromFile(imageToConvert);
        //    return ConvertToCCITTFaxTiff(img);
        //}

        public static Image ConvertToCcittFaxTiff(Image image)
        {
            Bitmap bmg = GetAsBitmap(image, 300);

            //  bmg = CropUnwantedBackground(bmg);
            // bmg = ConvertGrayscale(bmg);
            //  bmg = ResizeImage(bmg, 300, 85);
            Bitmap bitonalBmp = ConvertToBitonal(bmg);

            ImageCodecInfo codecInfo = GetCodecInfoForName("TIFF");

            EncoderParameters encoderParams = new EncoderParameters(2);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 08L);
            encoderParams.Param[1] = new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.CompressionCCITT4);

            MemoryStream ms = new MemoryStream();
            bitonalBmp.Save(ms, codecInfo, encoderParams);
            bitonalBmp.Dispose();
            return Image.FromStream(ms);
        }

        public static Image ConvertToImage(string imageToConvert, string codecName, long quality, int dpi)
        {
            Image img = Image.FromFile(imageToConvert);
            return ConvertToImage(img, codecName, quality, dpi);
        }

        public static Image ConvertToImage(Image imageToConvert, string codecName, long quality, int dpi)
        {
            Bitmap Bmp = GetAsBitmap(imageToConvert, dpi); // AForge.Imaging.Image.Clone((Bitmap)ResizeImage(imageToConvert, Dpi, 60));

            ImageCodecInfo codecInfo = GetCodecInfoForName(codecName);

            EncoderParameters encoderParams = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}
            };


            Bitmap newBitmap = new Bitmap(Bmp);

            newBitmap.SetResolution(dpi, dpi);

            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, codecInfo, encoderParams);
            Bmp.Dispose();
            newBitmap.Dispose();
            return Image.FromStream(ms);
        }

        public static Image ConvertToJpeg(Image imageToConvert, long quality, int dpi)
        {
            Bitmap Bmp = GetAsBitmap(imageToConvert, dpi); // AForge.Imaging.Image.Clone((Bitmap)ResizeImage(imageToConvert, Dpi, 60));
            Bmp.SetResolution(dpi, dpi);
            ImageCodecInfo codecInfo = GetCodecInfoForName("JPEG");

            EncoderParameters encoderParams = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}
            };


            MemoryStream ms = new MemoryStream();

            Bmp.Save(ms, codecInfo, encoderParams);
            Bmp.Dispose();
            return Image.FromStream(ms);
        }

        public static Image ConvertToPng(Image imageToConvert, long quality, int dpi)
        {
            Bitmap Bmp = GetAsBitmap(imageToConvert, dpi); // AForge.Imaging.Image.Clone((Bitmap)ResizeImage(imageToConvert, Dpi, 60));
            Bmp.SetResolution(dpi, dpi);
            ImageCodecInfo codecInfo = GetCodecInfoForName("PNG");

            EncoderParameters encoderParams = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}
            };


            MemoryStream ms = new MemoryStream();

            //Bmp = ResizeImage (Bmp, Dpi, 80);
            Bmp.Save(ms, codecInfo, encoderParams);
            Bmp.Dispose();
            return Image.FromStream(ms);
        }

        public static Bitmap ConvertToRGB(Bitmap original)
        {
            Bitmap newImage = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
            newImage.SetResolution(original.HorizontalResolution, original.VerticalResolution);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImageUnscaled(original, 0, 0);
            }
            return newImage;
        }

        public static Bitmap CropBorders(int top, int bottom, int left, int right, Bitmap bmp)
        {
            BBox b = new BBox();
            b.Height = bmp.Height - (top + bottom);
            b.Width = bmp.Width - (left + right);
            b.Left = left;
            b.Top = top;

            return CropToRectangle(b, bmp);
        }

        public static Bitmap CropToRectangle(BBox b, Bitmap image)
        {
            Bitmap bmpImage = new Bitmap(image);
            Bitmap bmpCrop = bmpImage.Clone(b.Rectangle, bmpImage.PixelFormat);
            return bmpCrop;
        }

        public static Bitmap DeskewImage(Bitmap bmp)
        {
            Deskew d = new Deskew(bmp);
            double a = d.GetSkewAngle();
            return Deskew.RotateImage(bmp, a);
        }

        public static Bitmap GetAsBitmap(Image image, int Dpi)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
                bmp.SetResolution(Dpi, Dpi);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));

                return bmp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Bitmap GetAsUnindexedBitmap(Image image)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                bmp.SetResolution(image.HorizontalResolution * 2, image.VerticalResolution * 2);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                return bmp;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ImageCodecInfo GetCodecInfoForName(string codecType)
        {
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();

            return (from t in info let enumName = codecType
                    where t.FormatDescription.Equals(enumName) select t).FirstOrDefault();
        }

        public static Image GetImageAt(string inputImage, int pagenum)
        {
            Image image = Image.FromFile(inputImage);

            Guid objGuid = image.FrameDimensionsList[0];
            FrameDimension frameDim = new FrameDimension(objGuid);

            for (int i = 0; i < image.GetFrameCount(frameDim); i++)
                if (i == pagenum - 1)
                {
                    image.SelectActiveFrame(frameDim, i);

                    return image;
                }

            return null;
        }

        public static unsafe bool IsGrayScale(Image image)
        {
            using (Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0);
                }

                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                int* pt = (int*) data.Scan0;
                bool res = true;

                for (int i = 0; i < data.Height * data.Width; i++)
                {
                    Color color = Color.FromArgb(pt[i]);

                    if (color.A == 0 || (color.R == color.G && color.G == color.B))
                        continue;
                    res = false;
                    break;
                }

                bmp.UnlockBits(data);

                return res;
            }
        }

      

        public static Bitmap ResizeImage(Image imgToResize, int resolution, int Percent)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            decimal percent = Percent / 100.00M;
            int destWidth = sourceWidth;
            int destHeight = sourceHeight;
            if (sourceWidth > 5)
                destWidth = (int) (sourceWidth * percent);

            if (sourceHeight > 5)
                destHeight = (int) (sourceHeight * percent);

            Bitmap b = new Bitmap(destWidth, destHeight);

            Graphics g = Graphics.FromImage(b);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            b.SetResolution(resolution, resolution);
            return b;
        }

        #region Private Methods

        #region GetImageBounds

        private static Point[] GetImageBounds(Bitmap bmp, Color? backColor)
        {
            //--------------------------------------------------------------------
            // Finding the Bounds of Crop Area bu using Unsafe Code and Image Proccesing
            Color c;
            int width = bmp.Width, height = bmp.Height;
            bool upperLeftPointFounded = false;
            Point[] bounds = new Point[2];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                c = bmp.GetPixel(x, y);
                bool sameAsBackColor = c.R <= backColor.Value.R * 1.1 && c.R >= backColor.Value.R * 0.9 && c.G <= backColor.Value.G * 1.1 &&
                                       c.G >= backColor.Value.G * 0.9 && c.B <= backColor.Value.B * 1.1 && c.B >= backColor.Value.B * 0.9;
                if (!sameAsBackColor)
                    if (!upperLeftPointFounded)
                    {
                        bounds[0] = new Point(x, y);
                        bounds[1] = new Point(x, y);
                        upperLeftPointFounded = true;
                    }
                    else
                    {
                        if (x > bounds[1].X)
                            bounds[1].X = x;
                        else if (x < bounds[0].X)
                            bounds[0].X = x;
                        if (y >= bounds[1].Y)
                            bounds[1].Y = y;
                    }
            }
            return bounds;
        }

        #endregion

        #region GetMatchedBackColor

        private static Color? GetMatchedBackColor(Bitmap bmp)
        {
            // Getting The Background Color by checking Corners of Original Image
            Point[] corners = new[]
            {
                new Point(0, 0), new Point(0, bmp.Height - 1), new Point(bmp.Width - 1, 0), new Point(bmp.Width - 1, bmp.Height - 1)
            }; // four corners (Top, Left), (Top, Right), (Bottom, Left), (Bottom, Right)
            for (int i = 0; i < 4; i++)
            {
                int cornerMatched = 0;
                Color backColor = bmp.GetPixel(corners[i].X, corners[i].Y);
                for (int j = 0; j < 4; j++)
                {
                    Color cornerColor = bmp.GetPixel(corners[j].X, corners[j].Y); // Check RGB with some offset
                    if (cornerColor.R <= backColor.R * 1.1 && cornerColor.R >= backColor.R * 0.9 && cornerColor.G <= backColor.G * 1.1 &&
                        cornerColor.G >= backColor.G * 0.9 && cornerColor.B <= backColor.B * 1.1 && cornerColor.B >= backColor.B * 0.9)
                        cornerMatched++;
                }
                if (cornerMatched > 2)
                    return backColor;
            }
            return null;
        }

        #endregion

        #endregion
    }
}