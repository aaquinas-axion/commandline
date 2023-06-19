using System.Collections;
using System.Collections.Generic;
using CSharpx;

namespace CommandLine.Core
{
    /// <summary>
    /// Class comprising the resulting collections from <see cref="TokenPartitioner.PartitionTokensByType"/>
    /// </summary>
    public class PartitionTokensByTypeResults
    {
        internal PartitionTokensByTypeResults(
            IEnumerable<Token> switchTokens, 
            IEnumerable<Token> scalarTokens,
            IEnumerable<Token> sequenceTokens,
            IEnumerable<Token> nonOptionTokens)
        {
            SwitchTokens    = switchTokens.Memoize();
            ScalarTokens    = scalarTokens.Memoize();
            SequenceTokens  = sequenceTokens.Memoize();
            NonOptionTokens = nonOptionTokens.Memoize();
        }

        internal IEnumerable<Token> SwitchTokens { get; }

        internal IEnumerable<Token> ScalarTokens { get; }

        internal IEnumerable<Token> SequenceTokens { get; }

        internal IEnumerable<Token> NonOptionTokens { get; }

        public IEnumerable Switches { get; }

        public IEnumerable Scalares { get; }

        public IEnumerable Sequences { get; }

        public IEnumerable NonOptions { get; }
    }
}
