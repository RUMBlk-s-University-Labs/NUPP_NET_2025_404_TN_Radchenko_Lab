using Microsoft.AspNetCore.Mvc;
using Polls.Common;
using Polls.REST.Models;
using System.Linq;

namespace Polls.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankedPollsController : PollController<RankedPoll, RankedPollModel>
    {
        public RankedPollsController(
            ICrudServiceAsync<RankedPoll> crudService,
            ICrudServiceAsync<Person> personService,
            ICrudServiceAsync<Option> optionService
        ) : base(crudService, personService, optionService) {}

        protected override RankedPoll CreateNewPoll(string title)
        {
            return new RankedPoll(title); 
        }

        protected override RankedPollModel MapToModel(RankedPoll poll)
        {
            var votes = poll.GetVotes()
                .ToDictionary(
                    o => o.Key.Id,
                    o => o.Value.ToDictionary(v => v.Key, v => v.Value)
                );

            return new RankedPollModel
            {
                Id = poll.Id,
                Title = poll.Title,
                IsOngoing = poll.IsOngoing,
                prevResult = poll.PrevResult(),
                Options = poll.GetOptions().Select(o => new OptionModel
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToHashSet(),
                Votes = votes
            };
        }
    }
}
