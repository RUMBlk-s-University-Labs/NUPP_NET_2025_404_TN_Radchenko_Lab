using System.Text.Json;

namespace Polls.Common
{
    public interface ICrudService<T>
    {
        public void Create(T element);
        public T Read(Guid id);
        public IEnumerable<T> ReadAll();
        public void Update(T element);
        public void Remove(T element);
    }

    public class GenericCrudService<T> : ICrudService<T> where T : IIdentifiable
    {
        private Dictionary<Guid, T> storage = new();

        public void Create(T element)
        {
            if (storage.ContainsKey(element.Id))
            {
                throw new ArgumentException($"Елемент {element.Id} вже існує");
            }
            storage[element.Id] = element;
        }

        public T Read(Guid id)
        {
            if (!storage.TryGetValue(id, out var element))
            {
                throw new KeyNotFoundException($"Елемент {id} не знайдено");
            }
            return element;
        }

        public IEnumerable<T> ReadAll() => storage.Values;

        public void Update(T element)
        {
            if (!storage.ContainsKey(element.Id))
            {
                throw new KeyNotFoundException($"Елемент {element.Id} не знайдено");
            }
            storage[element.Id] = element;
        }

        public void Remove(T element)
        {
            if (!storage.Remove(element.Id))
            {
                throw new KeyNotFoundException($"Елемент {element.Id} не знайдено");
            }
        }

        public void Save(string FilePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(storage, options);
            File.WriteAllText(FilePath, json);
        }

        public void Load(string FilePath)
        {
            var json = File.ReadAllText(FilePath);
            var content = JsonSerializer.Deserialize<Dictionary<Guid, T>>(json);

            if (content == null) {
                storage.Clear();
                return;
            }

            storage = content;
        }
    }

}