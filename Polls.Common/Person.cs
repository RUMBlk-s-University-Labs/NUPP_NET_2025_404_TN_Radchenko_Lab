namespace Polls.Common
{
    public class Person : IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string? Name { get; protected set; }

        public Person(Guid id, string name)
        {
            Name = name;
            Id = id;
        }

        //Конструктор
        public Person(string name)
        {
            SetName(name);
        }

        static public Person CreateNew()
        {
            var name = RandomNames.RandomPersonName.Get();
            return new Person(name);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ім'я не може бути порожнім");
            }
            Name = name;
        }
    }
}