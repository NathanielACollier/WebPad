using System.Xml.Linq;

namespace WebPad.Utilities
{
    public static class XElementExtensions
    {
        public static string InnerXml(this XElement element)
        {
            var reader = element.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }
    }
}