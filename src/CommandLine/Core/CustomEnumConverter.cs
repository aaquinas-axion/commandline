using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace CommandLine.Core
{
    internal class CustomEnumConverter : EnumConverter
    {
        public CustomEnumConverter(
            Type conversionType, 
            StringComparer comparer) : base (conversionType)
        {
            ConversionType = conversionType;
            Comparer       = comparer;
        }

        private Type ConversionType { get; }

        /// <inheritdoc />
        protected override IComparer Comparer { get; }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            switch (value)
            {
                case string toParse :
                    return TypeConverter.ToEnum(toParse, ConversionType, Comparer.Equals(StringComparer.OrdinalIgnoreCase));
                default:
                    return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
