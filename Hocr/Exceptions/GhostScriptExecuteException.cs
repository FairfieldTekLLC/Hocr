using System;

namespace Net.FairfieldTek.Hocr.Exceptions
{
   public  class GhostScriptExecuteException:Exception
    {
        public GhostScriptExecuteException(string msg, Exception innerException) : base(msg, innerException)
        {

        }
    }
}
