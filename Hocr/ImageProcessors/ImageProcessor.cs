using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Net.FairfieldTek.Hocr.Exceptions;

namespace Net.FairfieldTek.Hocr.ImageProcessors
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
            const int threshold = 580;

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


        public static Image ConvertToCcittFaxTiff(Image image, int dpi)
        {
            ImageCodecInfo codecInfo = GetCodecInfoForName("TIFF");
            EncoderParameters encoderParams = new EncoderParameters(2)
            {
                Param = {[0] = new EncoderParameter(Encoder.Quality, 08L), [1] = new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.CompressionCCITT4)}
            };

            Bitmap bmg = GetAsBitmap(image, dpi);
            Bitmap bitonalBmp = ConvertToBitonal(bmg);
            MemoryStream ms = new MemoryStream();
            bitonalBmp.Save(ms, codecInfo, encoderParams);
            bitonalBmp.Dispose();
            bmg.Dispose();
            return Image.FromStream(ms);
        }

        public static Image ConvertToImage(Image imageToConvert, string codecName, long quality, int dpi)
        {
            ImageCodecInfo codecInfo = GetCodecInfoForName(codecName);
            EncoderParameters encoderParams = new EncoderParameters(1) {Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}};

            Bitmap bmp = GetAsBitmap(imageToConvert, dpi);
            Bitmap newBitmap = new Bitmap(bmp);
            newBitmap.SetResolution(dpi, dpi);
            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, codecInfo, encoderParams);
            bmp.Dispose();
            newBitmap.Dispose();
            return Image.FromStream(ms);
        }


        public static Bitmap GetAsBitmap(Image image, int dpi)
        {
            try
            {
                Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
                bmp.SetResolution(dpi, dpi);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
                }

                return bmp;
            }
            catch (Exception e)
            {
                throw new InvalidBitmapException(" Hocr.ImageProcessors.ImageProcessor - GetAsBitmap", e);
                
            }
        }


        public static ImageCodecInfo GetCodecInfoForName(string codecType)
        {
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
            return (from t in info
                let enumName = codecType
                where t.FormatDescription.Equals(enumName)
                select t).FirstOrDefault();
        }
    }
}