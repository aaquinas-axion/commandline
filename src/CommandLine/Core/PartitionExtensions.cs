// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;

namespace CommandLine.Core
{
    static class PartitionExtensions
    {
        public static YesNoCollection<T> PartitionByPredicate<T>(
            this IEnumerable<T> items,
            Func<T, bool> pred)
        {
            var result = new YesNoCollection<T>();
            foreach (T item in items) {
                IList<T> list = pred(item) ? result.Yes : result.No;
                list.Add(item);
            }

            return result;
        }
    }
}
