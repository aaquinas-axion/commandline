using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.Core
{
    public sealed class PartitionResult
        : IEquatable<PartitionResult>
    {
        internal PartitionResult(IEnumerable<KeyValuePair<string, ValueGroup>> valueGroups, IEnumerable<Token> valueStrings, IEnumerable<Token> errors)
        {
            ValueGroups = valueGroups;
            Values      = valueStrings;
            Errors      = errors;
        }

        public IEnumerable<KeyValuePair<string, ValueGroup>> ValueGroups { get; }

        internal IEnumerable<Token> Values { get; }

        public IEnumerable<string> ValueStrings => Values.Select(a => a.Text);

        internal IEnumerable<Token> Errors { get; }

        public IEnumerable<string> ErrorStrings => Errors.Select(a => a.Text);

        /// <inheritdoc />
        public bool Equals(PartitionResult other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return
                (ReferenceEquals(ValueGroups, other.ValueGroups) || ((ValueGroups?.SequenceEqual(other.ValueGroups)) ?? false))  &&
                (ReferenceEquals(Values,      other.Values) || ((Values?.SequenceEqual(other.Values)) ?? false))                 &&
                (ReferenceEquals(Errors, other.Errors) || ((Errors?.SequenceEqual(other.Errors)) ?? false));
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is PartitionResult other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ValueGroups != null
                    ? ValueGroups.Aggregate(0, (h, v) => h*397 ^ v.GetHashCode())
                    : 0);
                hashCode = (hashCode * 397) ^
                           (Values != null
                               ? Values.Aggregate(0, (h, v) => h *397 ^ v.GetHashCode())
                               : 0);
                hashCode = (hashCode * 397) ^
                           (Errors != null
                               ? Errors.Aggregate(0, (h, v) => h * 397 ^ v.GetHashCode())
                               : 0);
                return hashCode;
            }
        }
    }
}
