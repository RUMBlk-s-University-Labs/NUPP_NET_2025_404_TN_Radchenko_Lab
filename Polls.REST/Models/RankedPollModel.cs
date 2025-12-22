namespace Polls.REST.Models
{
    public class RankedPollModel : PollModel
    {
        public Dictionary<Guid, Dictionary<Guid, int>> Votes;
    }
}
