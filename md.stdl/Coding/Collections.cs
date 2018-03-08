using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using md.stdl.String;

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

        /// <summary>
        /// Fill a list or array from a starting point with the contents of another list or array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="from">The other list or array</param>
        /// <param name="start">Starting index offset</param>
        public static void Fill<T>(this IList<T> list, IList<T> from, int start = 0)
        {
            if(start >= list.Count) return;
            for (int i = 0; i < Math.Min(list.Count-start, from.Count); i++)
            {
                int ii = i + start;
                list[ii] = from[i];
            }
        }

        /// <summary>
        /// Object PAth Query. General purpose object path query backend for accessing data with path like string and regex
        /// </summary>
        /// <typeparam name="TSrc">The source object type which contains the queryable data</typeparam>
        /// <typeparam name="TData">The endpoint data type which is queried</typeparam>
        /// <param name="obj">The source object which contains the queryable data</param>
        /// <param name="path">The path with set separator. Each path component excluding the endpoint represents a source object level. The endpoint should yield the Data</param>
        /// <param name="results">A list containing the resulting Data</param>
        /// <param name="separator">The separator string used to distinguish path components</param>
        /// <param name="dataKeysQuery">(srcObject): possibleKeys; A function which returns the possible data key values for the next path component level</param>
        /// <param name="childrenKeysQuery">(srcObject): possibleKeys; A function which returns the possible children key values for the next path component level</param>
        /// <param name="dataFromKey">(srcObject, key): resultData; A function which returns data objects via an endpoint key </param>
        /// <param name="childrenFromKey">(srcObject, key): nextObjects; A function which returns the objects to be queried for the next component level</param>
        public static void Opaq<TSrc, TData>(
            this TSrc obj,
            string path,
            List<TData> results,
            string separator,
            Func<TSrc, IEnumerable<string>> dataKeysQuery,
            Func<TSrc, IEnumerable<string>> childrenKeysQuery,
            Func<TSrc, string, IEnumerable<TData>> dataFromKey,
            Func<TSrc, string, IEnumerable<TSrc>> childrenFromKey)
        {
            var levels = path.SplitIgnoringBetween(separator, "`");
            string nextpath = string.Join(separator, levels, 1, levels.Length - 1);

            void NextStep(string currkey)
            {
                foreach (var cobj in childrenFromKey(obj, currkey))
                {
                    cobj.Opaq(nextpath, results, separator, dataKeysQuery, childrenKeysQuery, dataFromKey, childrenFromKey);
                }
            }
            
            if (levels[0][0] == '`' && levels[0][levels[0].Length - 1] == '`')
            {
                string key = levels[0].Trim('`');
                Regex Pattern = new Regex(key);
                foreach (string k in levels.Length == 1 ? dataKeysQuery(obj) : childrenKeysQuery(obj))
                {
                    if (Pattern.Match(k).Value == string.Empty) continue;
                    if (levels.Length == 1)
                    {
                        results.AddRange(dataFromKey(obj, k));
                    }
                    else NextStep(k);
                }
            }
            else
            {
                if (levels.Length == 1)
                {
                    results.AddRange(dataFromKey(obj, levels[0]));
                    return;
                }
                NextStep(levels[0]);
            }
        }

        /// <summary>
        /// Object PAth Query. General purpose object path query backend for accessing data with path like string and regex
        /// </summary>
        /// <typeparam name="TSrc">The source object type which contains the queryable data</typeparam>
        /// <typeparam name="TData">The endpoint data type which is queried</typeparam>
        /// <param name="obj">The source object which contains the queryable data</param>
        /// <param name="path">The path with set separator. Each path component excluding the endpoint represents a source object level. The endpoint should yield the Data</param>
        /// <param name="separator">The separator string used to distinguish path components</param>
        /// <param name="dataKeysQuery">(srcObject): possibleKeys; A function which returns the possible data key values for the next path component level</param>
        /// <param name="childrenKeysQuery">(srcObject): possibleKeys; A function which returns the possible children key values for the next path component level</param>
        /// <param name="dataFromKey">(srcObject, key): resultData; A function which returns data objects via an endpoint key </param>
        /// <param name="childrenFromKey">(srcObject, key): nextObjects; A function which returns the objects to be queried for the next component level</param>
        /// <returns>A list containing the resulting Data</returns>
        public static List<TData> Opaq<TSrc, TData>(
            this TSrc obj,
            string path,
            string separator,
            Func<TSrc, IEnumerable<string>> dataKeysQuery,
            Func<TSrc, IEnumerable<string>> childrenKeysQuery,
            Func<TSrc, string, IEnumerable<TData>> dataFromKey,
            Func<TSrc, string, IEnumerable<TSrc>> childrenFromKey)
        {
            var res = new List<TData>();
            obj.Opaq(path, res, separator, dataKeysQuery, childrenKeysQuery, dataFromKey, childrenFromKey);
            return res;
        }

        /// <summary>
        /// Non recursive version of Opaq. General purpose object path query backend for accessing data with path like string and regex
        /// </summary>
        /// <typeparam name="TSrc">The source object type which contains the queryable data</typeparam>
        /// <typeparam name="TChild">The type of children</typeparam>
        /// <typeparam name="TData">The endpoint data type which is queried</typeparam>
        /// <param name="obj">The source object which contains the queryable data</param>
        /// <param name="path">The path with set separator. Each path component excluding the endpoint represents a source object level. The endpoint should yield the Data</param>
        /// <param name="results">A list containing the resulting Data</param>
        /// <param name="children">List of children which you would execute the recursive Opaq on</param>
        /// <param name="separator">The separator string used to distinguish path components</param>
        /// <param name="dataKeysQuery">(srcObject): possibleKeys; A function which returns the possible data key values for the next path component level</param>
        /// <param name="childrenKeysQuery">(srcObject): possibleKeys; A function which returns the possible children key values for the next path component level</param>
        /// <param name="dataFromKey">(srcObject, key): resultData; A function which returns data objects via an endpoint key </param>
        /// <param name="childrenFromKey">(srcObject, key): nextObjects; A function which returns the objects to be queried for the next component level</param>
        /// <remarks>Can be used for the situation when the first source element is not the same type as its children (for example a context or a container type). If the first level of the path is Data then children will be empty, otherwise when first level of the path is a Child, the results will be empty</remarks>
        public static string OpaqNonRecursive<TSrc, TChild, TData>(
            this TSrc obj,
            string path,
            List<TData> results,
            List<TChild> children,
            string separator,
            Func<TSrc, IEnumerable<string>> dataKeysQuery,
            Func<TSrc, IEnumerable<string>> childrenKeysQuery,
            Func<TSrc, string, IEnumerable<TData>> dataFromKey,
            Func<TSrc, string, IEnumerable<TChild>> childrenFromKey)
        {
            var levels = path.SplitIgnoringBetween(separator, "`");
            string nextpath = string.Join(separator, levels, 1, levels.Length - 1);

            if (levels[0][0] == '`' && levels[0][levels[0].Length - 1] == '`')
            {
                string key = levels[0].Trim('`');
                Regex Pattern = new Regex(key);
                foreach (string k in levels.Length == 1 ? dataKeysQuery(obj) : childrenKeysQuery(obj))
                {
                    if (Pattern.Match(k).Value == string.Empty) continue;
                    if (levels.Length == 1)
                        results.AddRange(dataFromKey(obj, k));
                    else children.AddRange(childrenFromKey(obj, k));
                }
            }
            else
            {
                if (levels.Length == 1)
                    results.AddRange(dataFromKey(obj, levels[0]));
                else children.AddRange(childrenFromKey(obj, levels[0]));
            }
            return nextpath;
        }
    }
}
