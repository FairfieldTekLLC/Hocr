using System.Collections.Generic;
using System.Linq;

namespace Net.FairfieldTek.Hocr.HocrElements
{
    internal class HWord : HOcrClass
    {
        public HWord() { Characters = new List<HChar>(); }

        public IList<HChar> Characters { get; set; }

        public void AlignCharacters()
        {
            if (Characters.Count == 0)
                return;

            float maxHeight = Characters.OrderByDescending(x => x.BBox.Height).Take(1).Single().BBox.Height;
            float top = Characters.Select(x => x.BBox.Top).Min();
            foreach (HChar c in Characters)
            {
                c.BBox.Height = maxHeight;
                c.BBox.Top = top;
            }
        }
    }
}