using System.Collections.Generic;

namespace Hocr.HocrElements
{
    internal class HDocument : HOcrClass
    {
        private readonly Parser Parser = new Parser();

        public HDocument() { Pages = new List<HPage>(); }

        public IList<HPage> Pages { get; set; }

        public void AddFile(string hocrFile) { Parser.ParseHocr(this, hocrFile, true); }
    }
}