// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using CSharpx;

using SysTypeConverter = System.ComponentModel.TypeConverter;

namespace CommandLine.Core
{
    struct TypeDescriptor
    {
        private readonly TargetType              targetType;
        private readonly Maybe<int>              maxItems;
        private readonly Maybe<TypeDescriptor>   nextValue;
        private readonly Maybe<SysTypeConverter> typeConverter;

        private TypeDescriptor(
            TargetType            targetType,
            Maybe<int>            maxItems,
            Maybe<TypeDescriptor> nextValue,
            Maybe<SysTypeConverter> typeConverter)
        {
            this.targetType    = targetType;
            this.maxItems      = maxItems;
            this.nextValue     = nextValue;
            this.typeConverter = typeConverter;
        }

        public TargetType TargetType
        {
            get { return targetType; }
        }

        public Maybe<int> MaxItems
        {
            get { return maxItems; }
        }

        public Maybe<TypeDescriptor> NextValue
        {
            get { return this.nextValue; }
        }

        public Maybe<SysTypeConverter> TypeConverter
        {
            get { return this.typeConverter; }
        }

        public static TypeDescriptor Create(
            TargetType              tag,
            Maybe<int>              maximumItems,
            TypeDescriptor          nextValue = default(TypeDescriptor),
            Maybe<SysTypeConverter> typeConverter = default(Maybe<SysTypeConverter>))
        {
            if (maximumItems == null)
                throw new ArgumentNullException("maximumItems");

            return new TypeDescriptor(tag, maximumItems, nextValue.ToMaybe(), typeConverter??Maybe.Nothing<SysTypeConverter>());
        }
    }

    static class TypeDescriptorExtensions
    {
        public static TypeDescriptor WithNextValue(this TypeDescriptor descriptor, Maybe<TypeDescriptor> nextValue)
        {
            return TypeDescriptor.Create(
                descriptor.TargetType,
                descriptor.MaxItems,
                nextValue.GetValueOrDefault(default(TypeDescriptor)),
                descriptor.TypeConverter);
        }
    }
}
