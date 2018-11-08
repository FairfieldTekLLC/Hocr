using System;

namespace Net.FairfieldTek.Hocr.Exceptions
{
    public class PageCountMismatchException : Exception
    {
        public int PageCountStart { get; }
        public int PageCountEnd { get; }

        public PageCountMismatchException(string msg, int pageCountStart, int pageCountEnd) : base(msg)
        {
            PageCountStart = pageCountStart;
            PageCountEnd = pageCountEnd;
        }

    }
}
