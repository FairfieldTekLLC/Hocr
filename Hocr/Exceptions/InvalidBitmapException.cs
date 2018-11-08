using System;

namespace Net.FairfieldTek.Hocr.Exceptions
{
   public class InvalidBitmapException:Exception
    {
        public InvalidBitmapException(string msg, Exception inner) : base(msg, inner)
        {

        }
    }
}
