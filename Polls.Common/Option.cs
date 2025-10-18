using System.Text.Json.Serialization;

namespace Polls.Common
{
    public class Option : IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }
        
        //Конструктор
        public Option(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ім'я не може бути порожнім");
            }
            Name = name;
        }

        //Конструктор
        public Option(Guid id)
        {
            Id = id;
            Name = "ONLY FOR ID COMPARISON!!!";
        }

        //Конструктор для десеріалізації
        [JsonConstructor]
        public Option(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is Option other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static Option CreateNew()
        {
            return new Option(RandomNames.RandomString.Get(12));
        }
    }
}
