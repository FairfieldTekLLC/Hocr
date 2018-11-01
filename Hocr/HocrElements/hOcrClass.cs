namespace Net.FairfieldTek.Hocr.HocrElements
{
    internal class HOcrClass
    {
        public BBox BBox { get; set; }
        public string ClassName { get; set; }
        public string Id { get; set; }

        public string Text { get; set; }

        public string TextUnescaped => Text.Trim().Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&#39;", "'")
            .Replace("&#44;", "-").Replace("Ã¢â‚¬â€", "-").Replace("â€", "-").Replace("\r\n", string.Empty);

        public void CleanText()
        {
            if (Text == null)
                return;

            string results = Text.Trim().Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&#39;", "'")
                .Replace("&#44;", "-").Replace("Ã¢â‚¬â€", "-").Replace("â€", "-").Replace("\r\n", string.Empty);

            Text = results;
        }

        public override string ToString() { return string.Concat("Id: ", Id, "[", BBox.ToString(), "] Text: ", Text); }
    }
}