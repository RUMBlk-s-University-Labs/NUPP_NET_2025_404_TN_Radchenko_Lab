namespace Polls.REST.Models
{
    public class OptionModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateOptionModel
    {
        public string Name { get; set; }
    }

    
}
