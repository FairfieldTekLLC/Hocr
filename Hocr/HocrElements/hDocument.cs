using System.Collections.Generic;

namespace Net.FairfieldTek.Hocr.HocrElements
{
    internal class HDocument : HOcrClass
    {
        private readonly Parser _parser;

        public HDocument(float dpi)
        {
            Pages = new List<HPage>();
            _parser = new Parser(dpi);
        }

        public IList<HPage> Pages { get; set; }

        public void AddFile(string hocrFile) { _parser.ParseHocr(this, hocrFile, true); }
    }
}