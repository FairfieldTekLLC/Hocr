using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Hocr.Pdf;

namespace Hocr.ImageProcessors
{
    internal class ImageMagick 
    {
        public const string Mogrify = "mogrify";
		const string convert = "convert";

        public static string ConvertToGrayScale(string image)
        {
            string args = string.Concat(" -auto-level -colorspace gray -type grayscale -density 300 -gaussian-blur 0.05 ", image);

            RunCommand(Mogrify, args);

            return image;
        }

        /// <summary>
        /// Convert the specified input image to another format.
        /// </summary>
        /// <param name='input'>
        /// Input.
        /// </param>
        /// <param name="type"></param>
        /// <param name="sessionName"></param>
        /// <returns>The path to the newly converted image</returns>
        public static string Convert (Image input, PdfImageType type,string sessionName)
		{
            string imageFile = TempData.Instance.CreateTempFile(sessionName,".bmp");
			input.Save(imageFile, ImageFormat.Bmp);
			string output = Convert(imageFile, type);
            //TempData.Instance.Cleanup(imageFile);
			return output;
		}

        /// <summary>
        /// Convert the specified input image to another format.
        /// </summary>
        /// <param name='input'>
        /// Input.
        /// </param>
        /// <param name="type"></param>
        /// <returns>The path to the newly converted image</returns>
        public static string Convert (string input, PdfImageType type)
		{

			string output = input.Replace(Path.GetExtension(input), "." + type.ToString().ToLower());
			string quality = "";
			if(type == PdfImageType.Jpg || type == PdfImageType.Png)
				quality = "-quality 80 ";
			//-colorspace RGB -auto-level 
			string args = string.Concat(" -colorspace RGB -auto-level -strip -gaussian-blur 0.05 -density 600 ", quality, " ", input, " ", output, "\"");

		    RunCommand(string.Equals(Path.GetExtension(input), Path.GetExtension(output), StringComparison.CurrentCultureIgnoreCase) ? Mogrify : convert, args);

		    return output;
		}
        
        public static string Deskew(string image)
        {
            string args = string.Concat(" -background white -deskew 40% ", image);

            RunCommand(Mogrify, args);

            return image;
        }

        public static string Despeckle(string image)
        {
            string args = string.Concat(" -despeckle ", image);

            RunCommand(Mogrify, args);

            return image;
        }

        private static void RunCommand(string command, string args)
        {

			
				command = "cmd.exe";
				args = "/C " + Mogrify + " " + args;
			
            Process p = new Process();
            ProcessStartInfo s = new ProcessStartInfo(command, args)
            {
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            p.StartInfo = s;

            p.Start();
            p.WaitForExit();
        }
    }
}
