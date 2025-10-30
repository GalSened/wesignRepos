using Common.Interfaces.PDF;

namespace Common.Models.Files.PDF
{
    public class BaseField : IBaseField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Mandatory { get; set; }
        public int Page { get; set; }
    }
    public interface IBaseField { }
}
