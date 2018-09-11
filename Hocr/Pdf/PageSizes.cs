using System;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.error_messages;

namespace Hocr.Pdf
{
    internal class PageSize
    {
        /** This is the 11x17 format */
        public static readonly Rectangle _11X17 = new RectangleReadOnly(792, 1224);

        /** This is the a0 format */
        public static readonly Rectangle A0 = new RectangleReadOnly(2384, 3370);

        /** This is the a1 format */
        public static readonly Rectangle A1 = new RectangleReadOnly(1684, 2384);

        /** This is the a10 format */
        public static readonly Rectangle A10 = new RectangleReadOnly(73, 105);

        /** This is the a2 format */
        public static readonly Rectangle A2 = new RectangleReadOnly(1191, 1684);

        /** This is the a3 format */
        public static readonly Rectangle A3 = new RectangleReadOnly(842, 1191);

        /** This is the a4 format */
        public static readonly Rectangle A4 = new RectangleReadOnly(595, 842);

        /** This is the a5 format */
        public static readonly Rectangle A5 = new RectangleReadOnly(420, 595);

        /** This is the a6 format */
        public static readonly Rectangle A6 = new RectangleReadOnly(297, 420);

        /** This is the a7 format */
        public static readonly Rectangle A7 = new RectangleReadOnly(210, 297);

        /** This is the a8 format */
        public static readonly Rectangle A8 = new RectangleReadOnly(148, 210);

        /** This is the a9 format */
        public static readonly Rectangle A9 = new RectangleReadOnly(105, 148);

        /** This is the archA format */
        public static readonly Rectangle ARCH_A = new RectangleReadOnly(648, 864);

        /** This is the archB format */
        public static readonly Rectangle ARCH_B = new RectangleReadOnly(864, 1296);

        /** This is the archC format */
        public static readonly Rectangle ARCH_C = new RectangleReadOnly(1296, 1728);

        /** This is the archD format */
        public static readonly Rectangle ARCH_D = new RectangleReadOnly(1728, 2592);

        /** This is the archE format */
        public static readonly Rectangle ARCH_E = new RectangleReadOnly(2592, 3456);

        /** This is the b0 format */
        public static readonly Rectangle B0 = new RectangleReadOnly(2834, 4008);

        /** This is the b1 format */
        public static readonly Rectangle B1 = new RectangleReadOnly(2004, 2834);

        /** This is the b10 format */
        public static readonly Rectangle B10 = new RectangleReadOnly(87, 124);

        /** This is the b2 format */
        public static readonly Rectangle B2 = new RectangleReadOnly(1417, 2004);

        /** This is the b3 format */
        public static readonly Rectangle B3 = new RectangleReadOnly(1000, 1417);

        /** This is the b4 format */
        public static readonly Rectangle B4 = new RectangleReadOnly(708, 1000);

        /** This is the b5 format */
        public static readonly Rectangle B5 = new RectangleReadOnly(498, 708);

        /** This is the b6 format */
        public static readonly Rectangle B6 = new RectangleReadOnly(354, 498);

        /** This is the b7 format */
        public static readonly Rectangle B7 = new RectangleReadOnly(249, 354);

        /** This is the b8 format */
        public static readonly Rectangle B8 = new RectangleReadOnly(175, 249);

        /** This is the b9 format */
        public static readonly Rectangle B9 = new RectangleReadOnly(124, 175);

        /** This is the Crown Octavo format */
        public static readonly Rectangle CROWN_OCTAVO = new RectangleReadOnly(348, 527);

        /** This is the Crown Quarto format */
        public static readonly Rectangle CROWN_QUARTO = new RectangleReadOnly(535, 697);

        /** This is the Demy Octavo format */
        public static readonly Rectangle DEMY_OCTAVO = new RectangleReadOnly(391, 612);

        /** This is the Demy Quarto format. */
        public static readonly Rectangle DEMY_QUARTO = new RectangleReadOnly(620, 782);

        /** This is the executive format */
        public static readonly Rectangle EXECUTIVE = new RectangleReadOnly(522, 756);

        /** This is the American Foolscap format */
        public static readonly Rectangle FLSA = new RectangleReadOnly(612, 936);

        /** This is the European Foolscap format */
        public static readonly Rectangle FLSE = new RectangleReadOnly(648, 936);

        /** This is the halfletter format */
        public static readonly Rectangle HALFLETTER = new RectangleReadOnly(396, 612);

        /** This is the ISO 7810 ID-1 format (85.60 x 53.98 mm or 3.370 x 2.125 inch) */
        public static readonly Rectangle ID_1 = new RectangleReadOnly(242.65f, 153);

        /** This is the ISO 7810 ID-2 format (A7 rotated) */
        public static readonly Rectangle ID_2 = new RectangleReadOnly(297, 210);

        /** This is the ISO 7810 ID-3 format (B7 rotated) */
        public static readonly Rectangle ID_3 = new RectangleReadOnly(354, 249);

        /** This is the Large Crown Octavo format */
        public static readonly Rectangle LARGE_CROWN_OCTAVO = new RectangleReadOnly(365, 561);

        /** This is the Large Crown Quarto format */
        public static readonly Rectangle LARGE_CROWN_QUARTO = new RectangleReadOnly(569, 731);

        /** This is the ledger format */
        public static readonly Rectangle LEDGER = new RectangleReadOnly(1224, 792);

        /** This is the legal format */
        public static readonly Rectangle LEGAL = new RectangleReadOnly(612, 1008);

        // membervariables

        /** This is the letter format */
        public static readonly Rectangle LETTER = new RectangleReadOnly(612, 792);

        /** This is the note format */
        public static readonly Rectangle NOTE = new RectangleReadOnly(540, 720);

        /** This is the Penguin large paparback format. */
        public static readonly Rectangle PENGUIN_LARGE_PAPERBACK = new RectangleReadOnly(365, 561);

        /** This is the Pengiun small paperback format. */
        public static readonly Rectangle PENGUIN_SMALL_PAPERBACK = new RectangleReadOnly(314, 513);

        /** This is the postcard format */
        public static readonly Rectangle POSTCARD = new RectangleReadOnly(283, 416);

        /** This is the Royal Octavo format. */
        public static readonly Rectangle ROYAL_OCTAVO = new RectangleReadOnly(442, 663);

        /** This is the Royal Quarto format. */
        public static readonly Rectangle ROYAL_QUARTO = new RectangleReadOnly(671, 884);

        /** This is the small paperback format. */
        public static readonly Rectangle SMALL_PAPERBACK = new RectangleReadOnly(314, 504);

        /** This is the tabloid format */
        public static readonly Rectangle TABLOID = new RectangleReadOnly(792, 1224);

        /**
        * This method returns a Rectangle based on a String.
        * Possible values are the the names of a constant in this class
        * (for instance "A4", "LETTER",...) or a value like "595 842"
        */
        public static Rectangle GetRectangle(string name)
        {
            name = name.Trim().ToUpper(CultureInfo.InvariantCulture);
            int pos = name.IndexOf(' ');
            if (pos == -1)
                try
                {
                    return (Rectangle) typeof(PageSize).GetField(name).GetValue(null);
                }
                catch (Exception)
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.find.page.size.1", name));
                }

            try
            {
                string width = name.Substring(0, pos);
                string height = name.Substring(pos + 1);
                return new Rectangle(float.Parse(width, NumberFormatInfo.InvariantInfo), float.Parse(height, NumberFormatInfo.InvariantInfo));
            }
            catch (Exception e)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.page.size.format.2", name, e.Message));
            }
        }
    }
}