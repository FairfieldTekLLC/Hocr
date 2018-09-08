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

        public static Bitmap ConvertToBitonal(Bitmap original)
        {
            Bitmap source;

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

   
        public static Image ConvertToCcittFaxTiff(Image image)
        {
            Bitmap bmg = GetAsBitmap(image, 300);
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


        public static Image ConvertToImage(Image imageToConvert, string codecName, long quality, int dpi)
        {
            Bitmap bmp = GetAsBitmap(imageToConvert, dpi); 
            ImageCodecInfo codecInfo = GetCodecInfoForName(codecName);
            EncoderParameters encoderParams = new EncoderParameters(1)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}
            };
            Bitmap newBitmap = new Bitmap(bmp);
            newBitmap.SetResolution(dpi, dpi);
            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, codecInfo, encoderParams);
            bmp.Dispose();
            newBitmap.Dispose();
            return Image.FromStream(ms);
        }

      

        public static Bitmap CropToRectangle(BBox b, Bitmap image)
        {
            Bitmap bmpImage = new Bitmap(image);
            Bitmap bmpCrop = bmpImage.Clone(b.Rectangle, bmpImage.PixelFormat);
            return bmpCrop;
        }


        public static Bitmap GetAsBitmap(Image image, int dpi)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
                bmp.SetResolution(dpi, dpi);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));

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

       

        #region Private Methods

        #region GetImageBounds

        private static Point[] GetImageBounds(Bitmap bmp, Color? backColor)
        {
            //--------------------------------------------------------------------
            // Finding the Bounds of Crop Area bu using Unsafe Code and Image Proccesing
            int width = bmp.Width, height = bmp.Height;
            bool upperLeftPointFounded = false;
            Point[] bounds = new Point[2];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Color c = bmp.GetPixel(x, y);
                // ReSharper disable once PossibleInvalidOperationException
                bool sameAsBackColor = c.R <= backColor.Value.R * 1.1 && c.R >= backColor.Value.R * 0.9 && c.G <= backColor.Value.G * 1.1 &&
                                       c.G >= backColor.Value.G * 0.9 && c.B <= backColor.Value.B * 1.1 && c.B >= backColor.Value.B * 0.9;
                if (sameAsBackColor)
                    continue;

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