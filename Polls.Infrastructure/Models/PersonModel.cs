namespace Polls.Infrastructure.Models;
public class PersonModel : IIdentifiableModel
{
    public string? Name { get; set; }
    public ICollection<VoteModel> Votes { get; set; } = new List<VoteModel>();
}