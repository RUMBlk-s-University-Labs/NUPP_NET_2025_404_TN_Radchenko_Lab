namespace Polls.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

public class PersonModel : IdentityUser<Guid>, IIdentifiableModel
{
    public string? Name { get; set; }
    public ICollection<VoteModel> Votes { get; set; } = new List<VoteModel>();
}