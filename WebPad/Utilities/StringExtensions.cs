using System.Globalization;
using System.Linq;
using System.Text;

namespace WebPad.Utilities
{
    public static class StringExtensions
    {



        public static bool ContainedWithin(this string target, string source)
        {
            return source.Contains(target);
        }

        public static bool NotContainedWithin(this string target, string source)
        {
            return !ContainedWithin(target, source);
        }


        public static string AsTemplated(this string target, object replacements)
        {
            var properties = ReflectionUtilities.GetProperties(replacements);
            foreach (var property in properties)
            {
                target = target.Replace("{{" + property.Name + "}}", property.Value.ToString());
            }
            return target;
        }
    }
}