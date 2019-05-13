using System.Collections.Generic;

namespace joham.cs_futils
{
    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example: 
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            return MergeLeft(me, false, others);
        }
        public static T MergeLeft<T, K, V>(this T me, bool force, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            foreach (IDictionary<K, V> src in others)
            {
                foreach(KeyValuePair<K,V> p in src) 
                {
                    if (me.ContainsKey(p.Key) == false)
                        me[p.Key] = p.Value;
                    else if (force == true)
                        me[p.Key] = p.Value;
                }
            }
            return me;
        }

    }

}