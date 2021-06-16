using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Utils
{
    /// <summary>
    /// A dictionary data type that has two keys for each value.
    /// </summary>
    /// <typeparam name="Key1">The first key's data type.</typeparam>
    /// <typeparam name="Key2">The second key's data type.</typeparam>
    /// <typeparam name="Value">The value's data type.</typeparam>
    class MultiKeyDictionary<Key1, Key2, Value>
    {
        // Internally, stored as a dictionary to a dictionary
        private Dictionary<Key1, Dictionary<Key2, Value>> _dict;

        /// <summary>
        /// Instantiates the dictionary.
        /// </summary>
        public MultiKeyDictionary()
        {
            _dict = new Dictionary<Key1, Dictionary<Key2, Value>>();
        }

        /// <summary>
        /// Adds the specified key, key, and value triple to the dictionary.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <param name="val">The value.</param>
        public void Add(Key1 key1, Key2 key2, Value val)
        {
            if(!_dict.ContainsKey(key1))
                _dict[key1] = new Dictionary<Key2, Value>();

            _dict[key1][key2] = val;
        }

        /// <summary>
        /// Removes the specified key, key, and value triple from the dictionary.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns>True if the key/key/value triple was found and removed, or false if it was not found.</returns>
        public bool Remove(Key1 key1, Key2 key2)
        {
            if (!_dict.ContainsKey(key1) || !_dict[key1].ContainsKey(key2))
                return false;

            return _dict[key1].Remove(key2);
        }

        /// <summary>
        /// Returns whether the dictionary contains an entry for the specified keys.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns>True if a value exists that corresponds with the specified keys, false if not.</returns>
        public bool ContainsKeys(Key1 key1, Key2 key2)
        {
            if (_dict.ContainsKey(key1) && _dict[key1].ContainsKey(key2))
                return true;

            return false;
        }

        /// <summary>
        /// Indexing operator. Gets or sets the value associated with the specified key pair.
        /// </summary>
        /// <param name="key1">The first key.</param>
        /// <param name="key2">The second key.</param>
        /// <returns>The value associated with the key pair.</returns>
        public Value this[Key1 key1, Key2 key2]
        {
            get { return _dict[key1][key2]; }
            set { Add(key1, key2, value); }
        }
    }
}
