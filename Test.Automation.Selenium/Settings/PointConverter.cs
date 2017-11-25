using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents methods to convert a string to a Point object.
    /// </summary>
    public sealed class CustomPointConverter : ConfigurationConverterBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        internal bool ValidateType(object value, Type expected)
        {
            return (value == null) || (value.GetType() == expected);
        }

        /// <summary>
        /// Determines if the object can be converted to the given object.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        /// <summary>
        /// Determines if the object can be converted from a given object.
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
            ValidateType(value, typeof(Point));

            var x = ((Point) value).X;
            var y = ((Point) value).Y;

            return x.ToString(CultureInfo.InvariantCulture) + ", " + y.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the object from the given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ci"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (data == null) return null;

            var coordinates = data.ToString().Split(',').Select(int.Parse).ToArray();

            return new Point
            {
                X = coordinates[0],
                Y = coordinates[1]
            };
        }
    }
}