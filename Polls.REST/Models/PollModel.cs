namespace Polls.REST.Models
{
    public class PollModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public HashSet<OptionModel> Options { get; set; }
        public bool IsOngoing { get; set; }
        public Dictionary<Guid, int> prevResult { get; set; }
    }

    public class CreatePollModel
    {
        public string Title { get; set; }
        public HashSet<CreateOptionModel> Options { get; set; }
    }
}