using Polls.Common;

namespace Polls.Infrastructure.Models;

public class PollStatusModel : IIdentifiableModel
{
    public Guid PollId { get; set; }
    public PollModel? Poll { get; set; }
    public bool IsOngoing { get; set; } = false;
}