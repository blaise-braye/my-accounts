using System;
using System.Threading.Tasks;

namespace Operations.Classification.WpfUi.Technical.Caching
{
    public interface ICacheEntry
    {
        Task<object> GetOrSetAsync(Func<Task<object>> valueLoader);
    }
}