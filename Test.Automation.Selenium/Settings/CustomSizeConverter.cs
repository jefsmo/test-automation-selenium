using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents methods to convert a string to a Size object.
    /// </summary>
    public sealed class CustomSizeConverter : ConfigurationConverterBase
    {
        internal bool ValidateType(object value, Type expected)
        {
            return (value == null) || (value.GetType() == expected);
        }

        /// <summary>
        /// Determines if the object can be converted to the given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        /// <summary>
        /// Determines if the object can be converted from a given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        /// <summary>
        /// Converts the object to the given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ci"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            ValidateType(value, typeof(Size));

            var height = ((Size) value).Height;
            var width = ((Size) value).Width;

            return height.ToString(CultureInfo.InvariantCulture) + ", " + width.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the objecct from a given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ci"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (data == null) return null;

            var dimensions = data.ToString().Split(',').Select(int.Parse).ToArray();

            return new Size
            {
                Width = dimensions[0],
                Height = dimensions[1]
            };
        }
    }
}
