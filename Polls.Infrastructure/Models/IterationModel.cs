namespace Polls.Infrastructure.Models;

public class IterationModel : IIdentifiableModel
{
    public Guid PollId { get; set; }
    public PollModel? Poll {get; set;}
    public ICollection<OptionModel> Options { get; set; } = new List<OptionModel>();
    public ICollection<VoteModel> Votes { get; set; } = new List<VoteModel>();
}