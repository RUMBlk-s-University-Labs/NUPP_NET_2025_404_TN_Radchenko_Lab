using System.Text.Json;
using System.Collections;
using System.Collections.Concurrent;
namespace Polls.Common
{
    public interface ICrudServiceAsync<T> : IEnumerable<T>
    {
        public Task<bool> CreateAsync(T element);
        public Task<T> ReadAsync(Guid id);
        public Task<IEnumerable<T>> ReadAllAsync();
        public Task<IEnumerable<T>> ReadAllAsync(int page, int amount);
        public Task<bool> UpdateAsync(T element);
        public Task<bool> RemoveAsync(T element);
        public Task<bool> SaveAsync();
    }

    public class GenericCrudServiceAsync<T> : ICrudServiceAsync<T> where T : IIdentifiable
    {
        private ConcurrentDictionary<Guid, T> storage = new();

        private readonly object _createLock = new object();
        public Task<bool> CreateAsync(T element)
        {
            lock (_createLock) {
                var result = storage.TryAdd(element.Id, element);
                return Task.FromResult(result);
            }
        }

        public Task<T> ReadAsync(Guid id)
        {
            if (!storage.TryGetValue(id, out var element))
            {
                throw new KeyNotFoundException($"Елемент {id} не знайдено");
            }
            return Task.FromResult(element);
        }

        public Task<IEnumerable<T>> ReadAllAsync()
        {
            return Task.FromResult(storage.Values.AsEnumerable());
        }
        public Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
        {
            var result = storage.Values.Skip((page - 1) * amount).Take(amount);
            return Task.FromResult(result);
        }

        private readonly object _updateLock = new object();
        public Task<bool> UpdateAsync(T element)
        {
            lock (_updateLock)
            {
                if (!storage.ContainsKey(element.Id))
                {
                    throw new KeyNotFoundException($"Елемент {element.Id} не знайдено");
                }
                storage[element.Id] = element;
                return Task.FromResult(true);
            }
        }

        public Task<bool> RemoveAsync(T element)
        {
            return Task.FromResult(storage.TryRemove(element.Id, out _));
        }

        public Task<bool> SaveAsync()
        {
            var FilePath = "polls.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(storage, options);
            File.WriteAllTextAsync(FilePath, json);
            return Task.FromResult(true);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return storage.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}