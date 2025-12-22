namespace Polls.REST.Models
{
    public class VoteModel
    {
        public Guid PersonId { get; set; }
        public Guid OptionId { get; set; }
    }
}