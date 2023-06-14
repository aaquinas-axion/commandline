﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CSharpx;

namespace CommandLine.Core
{
    static class SpecificationExtensions
    {
        public static bool IsOption(this Specification specification)
        {
            return specification.Tag == SpecificationType.Option;
        }

        public static bool IsValue(this Specification specification)
        {
            return specification.Tag == SpecificationType.Value;
        }

        public static OptionSpecification WithLongName(this OptionSpecification specification, string newLongName)
        {
            return new OptionSpecification(
                specification.ShortName,
                newLongName,
                specification.Required,
                specification.SetName,
                specification.Min,
                specification.Max,
                specification.Separator,
                specification.DefaultValue,
                specification.HelpText,
                specification.MetaValue,
                specification.PossibleValues,
                specification.ConversionType,
                specification.TargetType,
                specification.Group,
                specification.FlagCounter,
                specification.Hidden,
                specification.TypeConverter);
        }

        public static Specification WithPossibleValues<tSpec>(
            this tSpec                 specification,
            ReadOnlyCollection<string> possibleValues)
            where tSpec : Specification
        {
            switch (specification)
            {
                case OptionSpecification optionSpecification:
                    return new OptionSpecification(
                        optionSpecification.ShortName,
                        optionSpecification.LongName,
                        optionSpecification.Required,
                        optionSpecification.SetName,
                        optionSpecification.Min,
                        optionSpecification.Max,
                        optionSpecification.Separator,
                        optionSpecification.DefaultValue,
                        optionSpecification.HelpText,
                        optionSpecification.MetaValue,
                        possibleValues,
                        optionSpecification.ConversionType,
                        optionSpecification.TargetType,
                        optionSpecification.Group,
                        optionSpecification.FlagCounter,
                        optionSpecification.Hidden,
                        optionSpecification.TypeConverter);
                case ValueSpecification valueSpecification:
                    return new ValueSpecification(
                        valueSpecification.Index,
                        valueSpecification.MetaName,
                        valueSpecification.Required,
                        valueSpecification.Min,
                        valueSpecification.Max,
                        valueSpecification.DefaultValue,
                        valueSpecification.HelpText,
                        valueSpecification.MetaValue,
                        possibleValues,
                        valueSpecification.ConversionType,
                        valueSpecification.TargetType,
                        valueSpecification.Hidden,
                        valueSpecification.TypeConverter);
                default:
                    throw new InvalidOperationException(
                        $"Unknown Specification type: {specification.GetType().FullName}");
            }
        }

        public static string UniqueName(this OptionSpecification specification)
        {
            return specification.ShortName.Length > 0 ? specification.ShortName : specification.LongName;
        }

        public static IEnumerable<Specification> ThrowingValidate(this IEnumerable<Specification> specifications, IEnumerable<Tuple<Func<Specification, bool>, string>> guardsLookup)
        {
            foreach (var guard in guardsLookup)
            {
                if (specifications.Any(spec => guard.Item1(spec)))
                {
                    throw new InvalidOperationException(guard.Item2);
                }
            }

            return specifications;
        }

        public static bool HavingRange(this Specification specification, Func<int, int, bool> predicate)
        {
            int min;
            int max;
            if (specification.Min.MatchJust(out min) && specification.Max.MatchJust(out max))
            {
                return predicate(min, max);
            }
            return false;
        }

        public static bool HavingMin(this Specification specification, Func<int, bool> predicate)
        {
            int min;
            if (specification.Min.MatchJust(out min))
            {
                return predicate(min);
            }
            return false;
        }

        public static bool HavingMax(this Specification specification, Func<int, bool> predicate)
        {
            int max;
            if (specification.Max.MatchJust(out max))
            {
                return predicate(max);
            }
            return false;
        }
    }
}
