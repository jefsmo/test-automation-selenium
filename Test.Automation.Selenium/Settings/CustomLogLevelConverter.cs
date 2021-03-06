﻿using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using OpenQA.Selenium;

namespace Test.Automation.Selenium.Settings
{
    /// <summary>
    /// Represents methods for convering LogLevel to and from string values.
    /// </summary>
    public sealed class CustomLogLevelConverter : ConfigurationConverterBase
    {
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
        /// <param name="value"></param>
        /// <returns></returns>
        public object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value)
        {
            ValidateType(value, typeof(LogLevel));

            return ((LogLevel)value).ToString();
        }

        /// <summary>
        /// Converts the object from the given type.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            if (data == null) return null;

            return Enum.Parse(typeof(LogLevel), data.ToString(), true);
        }
    }
}
