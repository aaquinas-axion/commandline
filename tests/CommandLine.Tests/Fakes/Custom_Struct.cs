// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using SysTypeConverter = System.ComponentModel.TypeConverter;

namespace CommandLine.Tests.Fakes
{
    public class CustomStructOptions
    {
        [Option('c', "custom", HelpText = "Custom Type")]
        public CustomStruct Custom { get; set; }
    }

    public struct CustomStruct
    {
        public string Input { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public CustomStruct(string url)
        {
            Input = url;
            Server = "";
            Port = 80;
            var data = url.Split(':');
            if (data.Length == 2)
            {
                Server = data[0];
                Port = Convert.ToInt32(data[1]);
            }
        }
    }

    public class CustomClassOptions
    {
        [Option('c', "custom", HelpText = "Custom Type")]
        public CustomClass Custom { get; set; }
    }

    public class CustomClass
    {
        public string Input { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public CustomClass(string url)
        {
            Input = url;
            Server = "";
            Port = 80;
            var data = url.Split(':');
            if (data.Length == 2)
            {
                Server = data[0];
                Port = Convert.ToInt32(data[1]);
            }
        }
    }

    public class CustomStructOptionsForTypeConverter
    {
        [Option('c', "custom", HelpText = "Custom Type", TypeConverter = typeof(CustomStructTypeConverter))]
        public CustomStructForTypeConverter Custom { get; set; }
    }

    public struct CustomStructForTypeConverter
    {
        public string Input  { get; set; }
        public string Server { get; set; }
        public int    Port   { get; set; }
    }

    public class CustomStructTypeConverter : SysTypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, 
            Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(
            ITypeDescriptorContext context, 
            CultureInfo culture, 
            object value)
        {
            if (!(value is string url))
                return base.ConvertFrom(context, culture, value);

            var data = url.Split(':');
            var custom = new CustomStructForTypeConverter
            {
                Input  = url,
                Server = data[0],
                Port   = 80,
            };

            if (data.Length == 2 && int.TryParse(data[1], out int port))
                custom.Port = port;
            return custom;
        }
    }

    
    public class CustomClassOptionsForTypeConverter
    {
        [Option('c', "custom", HelpText = "Custom Type", TypeConverter = typeof(CustomClassTypeConverter))]
        public CustomClassForTypeConverter Custom { get; set; }
    }

    public struct CustomClassForTypeConverter
    {
        public string Input  { get; set; }
        public string Server { get; set; }
        public int    Port   { get; set; }
    }

    public class CustomClassTypeConverter : SysTypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(
            ITypeDescriptorContext context, 
            Type                   sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(
            ITypeDescriptorContext context, 
            CultureInfo            culture, 
            object                 value)
        {
            if (!(value is string url))
                return base.ConvertFrom(context, culture, value);

            var data = url.Split(':');
            var custom = new CustomClassForTypeConverter
            {
                Input  = url,
                Server = data[0],
                Port   = 80,
            };

            if (data.Length == 2 && int.TryParse(data[1], out int port))
                custom.Port = port;
            return custom;
        }
    }
}
