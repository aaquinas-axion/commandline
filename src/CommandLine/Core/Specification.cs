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

        public Maybe<SysTypeConverter> GetConverter(bool useAppDomainTypeConverters, bool isFlag = false)
        {
            if (isFlag)
                return Maybe.Nothing<SysTypeConverter>();

            SysTypeConverter result = (!useAppDomainTypeConverters || ConversionType is null
                ? null
                : System.ComponentModel.TypeDescriptor.GetConverter(ConversionType)) as SysTypeConverter;

            if (typeConverter.IsJust())
            {
                var convType  = TypeConverter.FromJust();
                var info      = convType.GetTypeInfo();
                var ctorEmpty = info.GetConstructor(Type.EmptyTypes);
                var ctorType = info.GetConstructor(new Type[]{ typeof(Type) });
                var ctorString = info.GetConstructor(new Type[]{ typeof(string) });

                SysTypeConverter sysConv = null;

                if (!(ctorType is null))
                {
                    sysConv = ctorType?.Invoke(new object[]{ConversionType}) as SysTypeConverter;
                }
                if ((sysConv is null) && !(ctorString is null))
                {
                    sysConv = ctorString?.Invoke(new object[]{ConversionType.FullName}) as SysTypeConverter;
                }
                if ((sysConv is null) && !(ctorEmpty is null))
                {
                    sysConv = ctorString?.Invoke(new object[]{}) as SysTypeConverter;
                }
                if (sysConv is null)
                {
                    try
                    {
                        sysConv = Activator.CreateInstance(convType) as SysTypeConverter;
                    }
                    catch (TargetInvocationException)
                    {
                        // If we get here, we just can't construct it
                    }
                }

                result = !(sysConv is null || !sysConv.CanConvertFrom(typeof(string)))
                    ? sysConv
                    : null;
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

            bool isFlag    = spec.Tag == SpecificationType.Option && ((OptionSpecification)spec).FlagCounter;
            var  converter = spec.GetConverter(useAppDomainTypeConverters, isFlag);

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
