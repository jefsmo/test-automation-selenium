using System;
using System.ComponentModel;

namespace Test.Automation.Selenium
{
    /// <summary>
    /// Represents an extension method to display the string in the enum DescriptionAttribute.
    /// </summary>
    public static class ToDescriptionExtension
    {
        /// <summary>
        /// Displays the string in the enum DescriptionAttribute instead of the enum ToString() value.
        /// This allows friendly strings like 'Not Executed'.
        /// </summary>
        /// <param name="en">The enum whose description should be displayed.</param>
        /// <returns></returns>
        public static string ToDescription(this Enum en)
        {
            var type = en.GetType();
            var memberInfo = type.GetMember(en.ToString());

            if (memberInfo.Length <= 0) return en.ToString();

            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)attributes[0]).Description;
        }
    }
}
