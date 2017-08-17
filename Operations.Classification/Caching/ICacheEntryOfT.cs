using System;
using System.Threading.Tasks;

namespace Operations.Classification.Caching
{
    public interface ICacheEntry<TValue>
    {
        Task<TValue> GetAsync();

        Task<TValue> GetOrAddAsync(Func<Task<TValue>> valueLoader);

        Task<bool> DeleteAsync();

        Task<bool> SetAsync(TValue value);
    }
}