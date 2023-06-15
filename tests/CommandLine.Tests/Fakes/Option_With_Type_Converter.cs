using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Tests.Fakes
{
    [TypeConverter(typeof(Option_With_Type_Converter_Converter))]
    class Option_With_Type_Converter
    {
        public Option_With_Type_Converter(string value, string[] split)
        {
            Original = value;
            Split    = split;
        }

        public string Original { get; private set; }

        public string[] Split { get; private set; }
    }

    class Option_With_Type_Converter_Converter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string casted:
                    return new Option_With_Type_Converter(
                        casted,
                        casted.Split(','));
                default:
                    return base.ConvertFrom(context, culture, value);
            }
            
        }
    }
}
