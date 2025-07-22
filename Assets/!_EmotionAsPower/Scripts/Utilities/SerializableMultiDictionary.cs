using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;

namespace LgTyUtils
{
    [Serializable]
    public class SerializablMultiDictionary<K, V>
    {
        [Serializable]
        public struct KeyValuePair
        {
            public K key;
            public V value;

            public KeyValuePair(K key, V value)
            {
                this.key = key;
                this.value = value;
            }
        }

        [SerializeField]
        private KeyValuePair[] pairs = new KeyValuePair[0];

        private Dictionary<K, List<V>> dictionary;

        public Dictionary<K, List<V>> Dictionary
        {
            get 
            {
                if (dictionary == null) { 
                    RefreshDictionary();
                }
                return dictionary;
            }
        }
        private void RefreshDictionary()
        {
            dictionary = new Dictionary<K, List<V>>();
            for (int i = 0; i < pairs.Length; i++) {
                if (pairs[i].key != null) {
                    if (!dictionary.ContainsKey(pairs[i].key)) { 
                        dictionary[pairs[i].key] = new List<V>();
                    }
                    dictionary[pairs[i].key].Add(pairs[i].value);
                }
            }
        }

        public List<V> this[K key]
        {
            get
            {
                if (dictionary.ContainsKey(key))
                {
                    return new List<V>(Dictionary[key]); // Return a copy to prevent external modification
                }
                return new List<V>();
            }
        }

        public int Count => Dictionary.Count;

        public int TotalValueCount => Dictionary.Values.Sum(list => list.Count);

        public ICollection<K> Keys => Dictionary.Keys;

        public IEnumerable<V> Values => Dictionary.Values.SelectMany(list => list);

        public void Add(K key, V value) {
            if (!Dictionary.ContainsKey(key)) {
                Dictionary[key] = new List<V>();
            }
            Dictionary[key].Add(value);
            SyncArrayFromDictionary();
        }

        public void AddRange(K key, IEnumerable<V> values)
        {
            if (!Dictionary.ContainsKey(key)) {
                Dictionary[key] = new List<V>();
            }
            Dictionary[key].AddRange(values);
            SyncArrayFromDictionary();
        }

        public bool ContainsKey(K key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool ContainsValue(K key, V value)
        {
            return Dictionary.ContainsKey(key) && Dictionary[key].Contains(value);
        }

        public bool Remove(K key)
        {
            bool result = Dictionary.Remove(key);
            if (result)
            {
                SyncArrayFromDictionary();
            }
            return result;
        }

        public bool Remove(K key, V value)
        {
            if (Dictionary.ContainsKey(key))
            {
                bool removed = Dictionary[key].Remove(value);
                if (removed)
                {
                    if (Dictionary[key].Count == 0)
                    {
                        Dictionary.Remove(key);
                    }
                    return true;
                }
            }
            return false;
        }

        public int RemoveAll(K key, System.Predicate<V> match)
        {
            if (Dictionary.ContainsKey(key))
            {
                int removedCount = Dictionary[key].RemoveAll(match);
                if(removedCount > 0)
                {
                    if(Dictionary[key].Count == 0)
                    {
                        Dictionary.Remove(key);
                    }
                }
                return removedCount;
            }
            return 0;
        }

        public bool TryGetValues(K key, out List<V> values)
        {
            if (Dictionary.ContainsKey(key))
            {
                values = new List<V>(Dictionary[key]); // Return a copy to prevent modification
                return true;
            }
            values = new List<V>();
            return false;
        }

        public void Clear()
        {
            Dictionary.Clear();
            pairs = new KeyValuePair[0];
        }

        public void Clear(K key)
        {
            if (Dictionary.ContainsKey(key))
            {
                Dictionary.Remove(key);
                SyncArrayFromDictionary();
            }
        }

        public IEnumerator<System.Collections.Generic.KeyValuePair<K, List<V>>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public IEnumerable<System.Collections.Generic.KeyValuePair<K, V>> GetAllPairs()
        {
            foreach(var kvp in Dictionary)
            {
                foreach(var value in kvp.Value)
                {
                    yield return new System.Collections.Generic.KeyValuePair<K, V>(kvp.Key, value);
                }
            }
        }

        private void SyncArrayFromDictionary()
        {
            var allPairs = new List<KeyValuePair>();

            foreach (var pair in Dictionary)
            {
                foreach(var value in pair.Value)
                {
                    allPairs.Add(new KeyValuePair(pair.Key, value));
                }
            }

            pairs = allPairs.ToArray();
        }
    }
}
