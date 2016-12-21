namespace BloomFilters
{
    public interface IHashedDataStore
    {
        void Add(string item);
        bool Contains(string item);
    }
}