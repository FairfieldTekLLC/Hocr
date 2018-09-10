using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hocr.Enums
{
    public enum PdfImageType
    {
        /// <summary>
        /// Convert images to BMP
        /// </summary>
        Bmp,
        /// <summary>
        /// Convert images to JPG
        /// </summary>
        Jpg,
        /// <summary>
        /// Convert images to Tif
        /// </summary>
        Tif,
        /// <summary>
        /// Convert images to Png
        /// </summary>
        Png,
        /// <summary>
        /// Use JBIG2 Currently not 100%
        /// </summary>
        JBig2,
        /// <summary>
        /// Convert images to GIF
        /// </summary>
        Gif
    }
}
