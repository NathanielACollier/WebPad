using System.Collections.Generic;
using System.ComponentModel;

namespace WebPad.Utilities
{
    public class ReflectionUtilities
    {
        public sealed class PropertyValue
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        /// <summary>
        /// Given an anonymous type, GetProperties() will extract all
        /// the property names and values defined on the type and return
        /// them in a IEnumerable of strongly typed PropertyValue.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyValue> GetProperties(object o)
        {
            if (o == null)
                yield break;

            var props = TypeDescriptor.GetProperties(o);
            foreach (PropertyDescriptor prop in props)
            {
                var val = prop.GetValue(o);
                if (val == null) 
                    continue;
                yield return new PropertyValue {Name = prop.Name, Value = val};
            }
        }
    }
}