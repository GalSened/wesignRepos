using System.Drawing;

namespace Common.Models.XMLModels
{
    public class FieldCoordinate
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public string FontName { get; set; }
        public double TextSize { get; set; }
        public int Page { get; set; }

    }
}
