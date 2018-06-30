using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents methods to convert a string to a DriverType object.
    /// </summary>
    public sealed class CustomDriverTypeConverter : ConfigurationConverterBase
    {
        internal bool ValidateType(object value, Type expected)
        {
            return (value == null) || (value.GetType() == expected);
        }

        /// <summary>
        /// Determines if the object can be converted to the desired type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        /// <summary>
        /// Determines if the object can be converted from the given type.
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
            ValidateType(value, typeof(DriverType));

            return value.ToString();
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

            return (DriverType) Enum.Parse(typeof(DriverType), data.ToString(), true);
        }
    }
}
