namespace Polls.REST.Models
{
    public class PersonModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CreatePersonModel
    {
        public string Name { get; set; }
        public string Email { get; set;}
        public string Password { get; set; }
    }
}