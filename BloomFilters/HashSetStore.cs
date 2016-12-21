using System.Collections.Generic;

namespace BloomFilters
{
    public class HashSetStore : IHashedDataStore
    {
        private HashSet<string> HashSet { get; }

        public HashSetStore()
        {
            HashSet = new HashSet<string>();
        }

        public void Add(string item)
        {
            HashSet.Add(item);
        }

        public bool Contains(string item)
        {
            return HashSet.Contains(item);
        }
    }
}