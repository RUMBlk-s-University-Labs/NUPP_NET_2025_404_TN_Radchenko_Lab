namespace Polls.Infrastructure.Models;

public class OptionModel : IIdentifiableModel
{
    public ICollection<PollModel>? Polls { get; set; }
    public string? Name { get; set; }
}