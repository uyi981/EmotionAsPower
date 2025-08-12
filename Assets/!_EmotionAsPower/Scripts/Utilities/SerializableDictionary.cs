using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LgTyUtils
{
    [Serializable]
    public class SerializableDictionary<K, V> : ISerializationCallbackReceiver
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
        public KeyValuePair[] pairs = new KeyValuePair[0];

        private Dictionary<K, V> dictionary;

        private bool isDirty = false;

        public KeyValuePair[] Pairs
        {
            get
            {
                EnsureArraySynced();
                return pairs;
            }
            set
            {
                pairs = value;
                dictionary = null;
                isDirty = false;
            }
        }

        public Dictionary<K, V> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    RefreshDictionary();
                }
                return dictionary;
            }
        }

        private void RefreshDictionary()
        {
            dictionary = new Dictionary<K, V>();
            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i].key != null && !dictionary.ContainsKey(pairs[i].key))
                {
                    dictionary.Add(pairs[i].key, pairs[i].value);
                }
            }
            isDirty = false;
        }

        public V this[K key]
        {
            get { return dictionary[key]; }
            set
            {
                Dictionary[key] = value;
                isDirty = true;
            }
        }

        public int Count => Dictionary.Count;

        public ICollection<K> Keys => Dictionary.Keys;

        public ICollection<V> Values => Dictionary.Values;

        public void Add(K key, V value)
        {
            Dictionary.Add(key, value);
            isDirty = true;
        }

        public bool ContainsKey(K key)
        {
            return Dictionary.ContainsKey(key);
        }

        public bool Remove(K key)
        {
            bool result = Dictionary.Remove(key);
            if (result)
            {
                isDirty = true;
            }
            return result;
        }

        public bool TryGetValue(K key, out V value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            Dictionary.Clear();
            pairs = new KeyValuePair[0];
            isDirty = false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        private void EnsureArraySynced()
        {
            if (isDirty && dictionary != null)
            {
                SyncArrayFromDictionary();
            }
        }

        private void SyncArrayFromDictionary()
        {
            pairs = dictionary.Select(kvp => new KeyValuePair(kvp.Key, kvp.Value)).ToArray();
            isDirty = false;
        }

        public void OnBeforeSerialize()
        {
            EnsureArraySynced();
        }

        public void OnAfterDeserialize()
        {
            dictionary = null;
            isDirty = false;

            var seenKeys = new HashSet<K>();
            for (int i = 0; i < pairs.Length; i++)
            {
                if (pairs[i].key != null)
                {
                    if (seenKeys.Contains(pairs[i].key))
                    {
                        Debug.LogError($"Duplicate key found in SerializableDictionary: {pairs[i].key}");
                    }
                    else
                    {
                        seenKeys.Add(pairs[i].key);
                    }
                }
            }
        }

        public static SerializableDictionary<K, V> FromDictionary(Dictionary<K, V> dict)
        {
            var serializableDict = new SerializableDictionary<K, V>();
            serializableDict.pairs = dict.Select(kvp => new KeyValuePair(kvp.Key, kvp.Value)).ToArray();
            return serializableDict;
        }

        public Dictionary<K, V> ToDictionary()
        {
            return new Dictionary<K, V>(dictionary);
        }

        public IEnumerable<KeyValuePair> GetAllPairs()
        {
            EnsureArraySynced();
            return pairs;
        }

    }
}