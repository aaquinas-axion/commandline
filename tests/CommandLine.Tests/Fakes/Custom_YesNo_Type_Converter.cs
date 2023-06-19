using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Text;
using SysTypeConverter = System.ComponentModel.TypeConverter;

namespace CommandLine.Tests.Fakes
{
    internal class Custom_YesNo_Type_Converter
    {
        [Value(0, Default = false, HelpText = "First Value", TypeConverter = typeof(YesNoBoolConverter))]
        public bool First { get; set; }

        [Option('s', "Second", Default = true, HelpText = "Second Value", TypeConverter = typeof(YesNoBoolConverter))]
        public bool Second { get; set; }
    }

    public class YesNoBoolConverter : SysTypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is bool toFormat )
            {
                return toFormat
                    ? "Yes"
                    : "No";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string toParse)
            {
                switch (toParse.Trim())
                {
                    case "Yes":
                    {
                        return true;
                    }
                    case "No":
                    {
                        return false;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        private static bool Parse(string toParse, out object convertFrom)
        {
            

            convertFrom = null;
            return false;
        }

        /// <inheritdoc />
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(
                new string[]
                {
                    "Yes",
                    "No"
                });
        }

        /// <inheritdoc />
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
