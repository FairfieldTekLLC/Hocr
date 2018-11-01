using System.Collections.Generic;
using System.Linq;

namespace Net.FairfieldTek.Hocr.HocrElements
{
    /// <summary>
    ///     Represents one line of text in a paragraph.
    /// </summary>
    internal class HLine : HOcrClass
    {
        private readonly float _dpi;

        public HLine(float dpi)
        {
            Words = new List<HWord>();
            LinesInSameSentence = new List<HLine>();
            _dpi = dpi;
        }

        public IList<HLine> LinesInSameSentence { get; set; }

        public bool LineWasCombined { get; private set; }
        public IList<HWord> Words { get; set; }

        public void AlignTops()
        {
            if (Words.Count == 0)
                return;

            float maxHeight = Words.OrderByDescending(x => x.BBox.Height).Take(1).Single().BBox.Height;
            float top = Words.Select(x => x.BBox.Top).Min();

            foreach (HWord word in Words)
            {
                word.BBox.Top = top;
                word.BBox.Height = maxHeight;
            }
        }

        public HLine CombineLinesInSentence()
        {
            if (LinesInSameSentence == null || LinesInSameSentence.Count == 0)
                return this;
            HLine l = new HLine(_dpi)
            {
                Id = LinesInSameSentence.OrderBy(x => x.BBox.Left).First().Id,
                BBox = new BBox(_dpi)
                {
                    Top = LinesInSameSentence.Select(x => x.BBox.Top).Min(),
                    Height = LinesInSameSentence.Last().BBox.Height,
                    Left = LinesInSameSentence.Select(x => x.BBox.Left).Min(),
                    Width = LinesInSameSentence.Select(x => x.BBox.Width).Sum()
                }
            };

            foreach (HLine o in LinesInSameSentence.OrderBy(x => x.BBox.Left))
                l.Text += o.Text;

            if (LinesInSameSentence.Count > 1)
                l.LineWasCombined = true;
            return l;
        }
    }
}