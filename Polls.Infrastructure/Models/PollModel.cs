namespace Polls.Infrastructure.Models;

public class PollModel : IIdentifiableModel
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public ICollection<OptionModel> Options { get; set; } = new List<OptionModel>();
    public PollStatusModel? Status { get; set; }
    public IterationModel? Iteration { get; set; }
}