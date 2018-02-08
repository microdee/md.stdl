using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Coding
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Overload of LINQ select where you can have different mapping functions for keys and the values
        /// </summary>
        /// <typeparam name="TOldKey">Type of source key</typeparam>
        /// <typeparam name="TOldValue">Type of source value</typeparam>
        /// <typeparam name="TNewKey">Type of target key</typeparam>
        /// <typeparam name="TNewValue">Type of target value</typeparam>
        /// <param name="source">Source dictionary</param>
        /// <param name="keymapper">Key mapping function</param>
        /// <param name="valuemapper">Value mapping function</param>
        /// <returns>New dictionary with the target types</returns>
        public static Dictionary<TNewKey, TNewValue> Select<TOldKey, TOldValue, TNewKey, TNewValue>
        (
            this IDictionary<TOldKey, TOldValue> source,
            Func<TOldKey, TNewKey> keymapper,
            Func<TOldValue, TNewValue> valuemapper
        )
        {
            return (from kvp in source select new KeyValuePair<TNewKey, TNewValue>(
                    keymapper(kvp.Key),
                    valuemapper(kvp.Value)
                )).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
