using System.Collections.Generic;

namespace CommandLine.Core
{
    /// <summary>
    /// Pared list of items where one collection represents "Yes" Items and the Other "No"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class YesNoCollection<T>
    {
        public YesNoCollection()
        {
            Yes = new List<T>();
            No  = new List<T>();
        }

        /// <summary>
        /// "Yes" Collection
        /// </summary>
        internal IList<T> Yes { get; }

        /// <summary>
        /// "No" collection
        /// </summary>
        internal IList<T> No { get; }
    }
}
