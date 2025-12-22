namespace Polls.Common
{
    public class Person : IIdentifiable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; }

        //Конструктор
        public Person(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ім'я не може бути порожнім");
            }
            Name = name;
        }

        static public Person CreateNew()
        {
            var name = RandomNames.RandomPersonName.Get();
            return new Person(name);
        }
    }
}