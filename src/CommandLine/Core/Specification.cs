// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;
using CSharpx;

using SysTypeConverter = System.ComponentModel.TypeConverter;

namespace CommandLine.Core
{
    enum SpecificationType
    {
        Option,
        Value
    }

    enum TargetType
    {
        Switch,
        Scalar,
        Sequence
    }

    abstract class Specification
    {
        private readonly SpecificationType tag;
        private readonly bool required;
        private readonly bool hidden;
        private readonly Maybe<int> min;
        private readonly Maybe<int> max;
        private readonly Maybe<object> defaultValue;
        private readonly string helpText;
        private readonly string metaValue;
        private readonly IEnumerable<string> possibleValues;
        /// This information is denormalized to decouple Specification from PropertyInfo.
        private readonly Type conversionType;
        private readonly TargetType targetType;
        private readonly Maybe<Type> typeConverter;

        protected Specification(SpecificationType tag, bool required, Maybe<int> min, Maybe<int> max,
            Maybe<object> defaultValue, string helpText, string metaValue, IEnumerable<string> possibleValues,
            Type conversionType, TargetType targetType, bool hidden = false, Maybe<Type> typeConverter = default(Maybe<Type>))
        {
            this.tag            = tag;
            this.required       = required;
            this.min            = min;
            this.max            = max;
            this.defaultValue   = defaultValue;
            this.conversionType = conversionType;
            this.targetType     = targetType;
            this.typeConverter  = typeConverter ?? Maybe.Nothing<Type>();
            this.helpText       = helpText;
            this.metaValue      = metaValue;
            this.possibleValues = possibleValues;
            this.hidden         = hidden;
        }

        public SpecificationType Tag 
        {
            get { return tag; }
        }

        public bool Required
        {
            get { return required; }
        }

        public Maybe<int> Min
        {
            get { return min; }
        }

        public Maybe<int> Max
        {
            get { return max; }
        }

        public Maybe<object> DefaultValue
        {
            get { return defaultValue; }
        }

        public string HelpText
        {
            get { return helpText; }
        }

        public string MetaValue
        {
            get { return metaValue; }
        }

        public IEnumerable<string> PossibleValues
        {
            get { return possibleValues; }
        }

        public Type ConversionType
        {
            get { return conversionType; }
        }

        public TargetType TargetType
        {
            get { return targetType; }
        }

        public Maybe<Type> TypeConverter
        {
            get { return typeConverter; }
        }

        public bool Hidden
        {
            get { return hidden; }
        }

        public Maybe<SysTypeConverter> GetConverter(bool useAppDomainTypeConverters, StringComparer comparer)
        {
            SysTypeConverter result = (!useAppDomainTypeConverters || ConversionType is null
                ? null
                : System.ComponentModel.TypeDescriptor.GetConverter(ConversionType)) as SysTypeConverter;

            if (!(result is null) && ConversionType.IsEnum && result?.GetType() == typeof(EnumConverter))
                result = new CustomEnumConverter(ConversionType, comparer);

            if (typeConverter.IsJust())
            {
                var convType = TypeConverter.FromJust();
                var info     = convType.GetTypeInfo();
                var ctor     = info.GetConstructor(Type.EmptyTypes);
                var sysConv  = ctor?.Invoke(new object[0]) as SysTypeConverter;
                if (!(sysConv is null || !sysConv.CanConvertFrom(typeof(string))))
                    result = sysConv;
                else
                {
                    try
                    {
                        result = (Activator.CreateInstance(convType) as SysTypeConverter) ?? result;
                    }
                    catch (TargetInvocationException)
                    { }
                }
                    
            }

            if ((result is null || !result.CanConvertFrom(typeof(string))))
                return Maybe.Nothing<SysTypeConverter>();
            return result.ToMaybe();
        }

        public static Specification FromProperty(PropertyInfo property) =>
            FromProperty(property, false, StringComparer.Ordinal);

        public static Specification FromProperty(PropertyInfo property, bool useAppDomainTypeConverters, StringComparer comparer)
        {       
            var attrs = property.GetCustomAttributes(true);
            var oa    = attrs.OfType<OptionAttribute>();
            Specification spec  = null;
            if (oa.Count() == 1)
            {
                OptionSpecification ospec = OptionSpecification.FromAttribute(
                    oa.Single(),
                    property.PropertyType,
                    ReflectionHelper.GetNamesOfEnum(property.PropertyType));

                if (ospec.ShortName.Length == 0 && ospec.LongName.Length == 0)
                {
                    ospec = ospec.WithLongName(property.Name.ToLowerInvariant());
                }

                spec = ospec;
            }

            var va = attrs.OfType<ValueAttribute>();
            if (va.Count() == 1)
            {
                spec = ValueSpecification.FromAttribute(va.Single(), property.PropertyType,
                    property.PropertyType.GetTypeInfo().IsEnum
                        ? Enum.GetNames(property.PropertyType)
                        : Enumerable.Empty<string>());
            }
            if(spec is null)
                throw new InvalidOperationException();

            if (spec is null)
                throw new InvalidOperationException();
            var converter = spec.GetConverter(useAppDomainTypeConverters, comparer);

            if (!converter.IsJust())
                return spec;
            var conv = converter.FromJust();
            if (!conv.CanConvertFrom(typeof(string)) || !conv.GetStandardValuesSupported())
                return spec;
            var standard = conv.GetStandardValues();
            List<string> standardList = new List<string>();
            foreach (object val in standard)
                standardList.Add(val.ToString());

            return spec.WithPossibleValues(standardList.AsReadOnly());
        }
    }
}
