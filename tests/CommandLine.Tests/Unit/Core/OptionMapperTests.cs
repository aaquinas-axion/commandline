﻿// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if PLATFORM_DOTNET
using System.Reflection;
#endif
using Xunit;
using CSharpx;
using RailwaySharp.ErrorHandling;
using CommandLine.Core;
using CommandLine.Tests.Fakes;

using SysTypeConverter = System.ComponentModel.TypeConverter;
using ValueType = CommandLine.Core.ValueType;

namespace CommandLine.Tests.Unit.Core
{
    public class OptionMapperTests
    {
        [Fact]
        public void Map_boolean_switch_creates_boolean_value()
        {
            // Fixture setup
            var tokenPartitions = new[]
                {
                    new KeyValuePair<string,ValueGroup>("x", new ValueGroup(Token.Name("x"), ValueType.Switch, "true"))
                };
            var specProps = new[]
                {
                    SpecificationProperty.Create(
                        new OptionSpecification("x", string.Empty, false, string.Empty, Maybe.Nothing<int>(), Maybe.Nothing<int>(), '\0', Maybe.Nothing<object>(), string.Empty, string.Empty, new List<string>(), typeof(bool), TargetType.Switch, string.Empty),
                        typeof(Simple_Options).GetProperties().Single(p => p.Name.Equals("BoolValue", StringComparison.Ordinal)),
                        Maybe.Nothing<object>())
                };

            // Exercize system 
            var result = OptionMapper.MapValues(
                specProps.Where(pt => pt.Specification.IsOption()),
                tokenPartitions,
                false,
                (vals, type, converter, isScalar, isFlag) => TypeConverter.ChangeType(vals, type, isScalar, isFlag, CultureInfo.InvariantCulture, false, converter),
                StringComparer.Ordinal);

            // Verify outcome
            Assert.NotNull(((Ok<IEnumerable<SpecificationProperty>, Error>)result).Success.Single(
                a => a.Specification.IsOption()
                && ((OptionSpecification)a.Specification).ShortName.Equals("x")
                && (bool)((Just<object>)a.Value).Value));

            // Teardown
        }

        [Fact]
        public void Map_with_multi_instance_scalar()
        {
            var tokenPartitions = new[]
            {
                new KeyValuePair<string, ValueGroup>("s",            new ValueGroup(Token.Name("s"), ValueType.Scaler, "string1")),
                new KeyValuePair<string, ValueGroup>("shortandlong",            new ValueGroup(Token.Name("s"), ValueType.Scaler, "string2")),
                new KeyValuePair<string, ValueGroup>("shortandlong",            new ValueGroup(Token.Name("s"), ValueType.Scaler, "string3")),
                new KeyValuePair<string, ValueGroup>("s",            new ValueGroup(Token.Name("s"), ValueType.Scaler, "string4")),
                new KeyValuePair<string, ValueGroup>("s",            new ValueGroup(Token.Name("s"), ValueType.Scaler, "string1"))
            };

            var specProps = new[]
            {
                SpecificationProperty.Create(
                    new OptionSpecification("s", "shortandlong", false, string.Empty, Maybe.Nothing<int>(), Maybe.Nothing<int>(), '\0', Maybe.Nothing<object>(), string.Empty, string.Empty, new List<string>(), typeof(string), TargetType.Scalar, string.Empty),
                    typeof(Simple_Options).GetProperties().Single(p => p.Name.Equals(nameof(Simple_Options.ShortAndLong), StringComparison.Ordinal)),
                    Maybe.Nothing<object>()),
            };

            var result = OptionMapper.MapValues(
                specProps.Where(pt => pt.Specification.IsOption()),
                tokenPartitions,
                false,
                (vals, type, converter, isScalar, isFlag) => TypeConverter.ChangeType(vals, type, isScalar, isFlag, CultureInfo.InvariantCulture, false, converter),
                StringComparer.Ordinal);

            var property = result.SucceededWith().Single();
            Assert.True(property.Specification.IsOption());
            Assert.True(property.Value.MatchJust(out var stringVal));
            Assert.Equal(tokenPartitions.Last().Value.Values.Last(), stringVal);
        }

        [Fact]
        public void Map_with_multi_instance_sequence()
        {
            var tokenPartitions = new[]
            {
                new KeyValuePair<string, ValueGroup>("i", new ValueGroup(Token.Name("i"), ValueType.Scaler, "1")),
                new KeyValuePair<string, ValueGroup>("i", new ValueGroup(Token.Name("i"), ValueType.Scaler, "3")),
                new KeyValuePair<string, ValueGroup>("i", new ValueGroup(Token.Name("i"), ValueType.Scaler, "4", "5"))
            };
            var specProps = new[]
            {
                SpecificationProperty.Create(
                    new OptionSpecification("i", string.Empty, false, string.Empty, Maybe.Nothing<int>(), Maybe.Nothing<int>(), '\0', Maybe.Nothing<object>(), string.Empty, string.Empty, new List<string>(), typeof(IEnumerable<int>), TargetType.Sequence, string.Empty),
                    typeof(Simple_Options).GetProperties().Single(p => p.Name.Equals(nameof(Simple_Options.IntSequence), StringComparison.Ordinal)),
                    Maybe.Nothing<object>())
            };

            var result = OptionMapper.MapValues(
                specProps.Where(pt => pt.Specification.IsOption()),
                tokenPartitions,
                false,
                (vals, type, converter, isScalar, isFlag) => TypeConverter.ChangeType(vals, type, isScalar, isFlag, CultureInfo.InvariantCulture, false, converter),
                StringComparer.Ordinal);

            var property = result.SucceededWith().Single();
            Assert.True(property.Specification.IsOption());
            Assert.True(property.Value.MatchJust(out var sequence));

            var expected = tokenPartitions.Aggregate(Enumerable.Empty<int>(), (prev, part) => prev.Concat(part.Value.Values.Select(i => int.Parse(i))));
            Assert.Equal(expected, sequence);
        }
    }
}
