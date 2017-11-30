using System.Xml.Linq;

namespace WebPad.Utilities
{
    public static class XmlExtensions
    {
        public static string DefaultValueIfNull(this XAttribute target, string value)
        {
            return null == target ? value : target.Value;
        }

        public static string DefaultValueIfNull(this XElement target, string value)
        {
            return null == target ? value : target.Value;
        }
    }
}