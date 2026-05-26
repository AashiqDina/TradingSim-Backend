namespace TradingSimulatorBackend.Caching
{
    public class CacheEntry<T>
    {
        public T? Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}