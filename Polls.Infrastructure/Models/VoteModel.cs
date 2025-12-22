namespace Polls.Infrastructure.Models;

public class VoteModel : IIdentifiableModel
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public PollModel? Poll { get; set; }
    public Guid? PersonId { get; set; }
    public PersonModel? Person {get; set;}
    public Guid OptionId { get; set; }
    public OptionModel? Option { get; set; }
    public Guid IterationId {get; set;}
    public IterationModel? Iteration { get; set; }
    public int weight { get; set; } = 1;
}