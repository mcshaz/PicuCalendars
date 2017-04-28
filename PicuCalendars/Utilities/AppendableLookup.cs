using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.Utilities
{
    public class AppendableLookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<TKey, List<TElement>> _dict = new Dictionary<TKey, List<TElement>>();

        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                if (_dict.TryGetValue(key, out List<TElement> value)){
                    return value;
                }
                return Enumerable.Empty<TElement>();
            }
        }

        public int Count => _dict.Count;

        public bool Contains(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="element"></param>
        /// <returns>False if the IGrouping already exists</returns>
        public bool Add(TKey key, TElement element)
        {
            if (_dict.TryGetValue(key, out List<TElement> list))
            {
                list.Add(element);
                return false;
            }
            else
            {
                _dict.Add(key, new List<TElement>{ element});
                return true;
            }
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            // First iterate over the groupings in the dictionary, and then over the default-key 
            // grouping, if there is one. 

            foreach (IGrouping<TKey, TElement> grouping in _dict.Values) 
            {
                yield return grouping;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IGrouping<TKey, TElement>>)this).GetEnumerator();
        }
    }
}
