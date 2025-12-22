namespace Polls.REST.Models
{
    public class SingleVotePollModel : PollModel
    {
        public Dictionary<Guid, Guid> Votes;
    }
}
