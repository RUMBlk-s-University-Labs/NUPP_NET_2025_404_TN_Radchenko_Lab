using Microsoft.AspNetCore.Mvc;
using Polls.Common;
using Polls.REST.Models;
using System.Linq;

namespace Polls.REST.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SingleVotePollsController : PollController<SingleVotePoll, SingleVotePollModel>
    {
        public SingleVotePollsController(
            ICrudServiceAsync<SingleVotePoll> crudService,
            ICrudServiceAsync<Person> personService,
            ICrudServiceAsync<Option> optionService
        ) : base(crudService, personService, optionService) {}

        protected override SingleVotePoll CreateNewPoll(string title)
        {
            return new SingleVotePoll(title); 
        }

        protected override SingleVotePollModel MapToModel(SingleVotePoll poll)
        {
            var votes = poll.GetVotes()
                .ToDictionary(
                    o => o.Key.Id,
                    o => o.Value
                );

            return new SingleVotePollModel
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