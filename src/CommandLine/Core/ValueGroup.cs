// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using CommandLine.Infrastructure;
using CSharpx;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.Core
{

    public enum ValueType
    {
        Switch,
        Scaler,
        Sequence
    }

    public class ValueGroup
        : IEquatable<ValueGroup>
    {
        internal Token Origin { get; }

        public IEnumerable<string> Values { get; }

        public ValueType Class { get; }

        internal ValueGroup(Token origin, ValueType valueType, params string[] values)
        {
            Values = values;
            Class  = valueType;
        }

        internal static IEnumerable<KeyValuePair<string, ValueGroup>> ForSwitch(
            IEnumerable<Token> tokens)
        {
            return tokens.Select(t => new KeyValuePair<string, ValueGroup>(t.Text, new ValueGroup(t, ValueType.Switch, "true")));
        }

        internal static IEnumerable<KeyValuePair<string, ValueGroup>> ForScalar(
            IEnumerable<Token> tokens)
        {
            return tokens
                  .Group(2)
                  .Select(
                       (g) => new KeyValuePair<string, ValueGroup>(
                           g[0].Text,
                           new ValueGroup(g[0], ValueType.Scaler, g[1].Text)));
        }

        internal static IEnumerable<KeyValuePair<string, ValueGroup>> ForSequence(
            IEnumerable<Token> tokens)
        {
            return from t in tokens.Pairwise(
                (f, s) =>
                        f.IsName()
                            ? new KeyValuePair<string, ValueGroup>( f.Text, new ValueGroup(
                                                                        f,
                                                                        ValueType.Sequence,
                                                                        tokens.SkipWhile(t => !t.Equals(f)).SkipWhile(t => t.Equals(f)).TakeWhile(v => v.IsValue()).Select(x => x.Text).ToArray()))
                            : new KeyValuePair<string, ValueGroup>(string.Empty, new ValueGroup(f, ValueType.Sequence)))
                   where t.Key.Length > 0 && t.Value.Values.Any()
                   select t;
        }

        /// <inheritdoc />
        public bool Equals(ValueGroup other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(Origin, other.Origin) &&
                   ReferenceEquals(Values, other.Values) || (Values?.SequenceEqual(Values) ?? false) && 
                   Class == other.Class;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ValueGroup)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Origin != null
                    ? Origin.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^
                           (Values != null
                               ? Values.Aggregate(0, (h,v)=> h*397 ^ v.GetHashCode())
                               : 0);
                hashCode = (hashCode * 397) ^ (int)Class;
                return hashCode;
            }
        }
    }
}
