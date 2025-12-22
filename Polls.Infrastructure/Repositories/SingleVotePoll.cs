using Microsoft.EntityFrameworkCore;
using Polls.Common;
using Polls.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polls.Infrastructure.Repositories
{
    public class SingleVotePollRepository : IRepository<SingleVotePoll>
    {
        private readonly PollsContext _context;
        private readonly DbSet<SingleVotePollModel> _dbSet;
        private readonly PersonRepository _personRepository;

        public SingleVotePollRepository(PollsContext context, PersonRepository personRepository)
        {
            _context = context;
            _dbSet = context.Set<SingleVotePollModel>();
            _personRepository = personRepository;
        }


        public async Task<SingleVotePoll> GetByIdAsync(Guid id)
        {
            var model = await GetBaseQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (model == null) return null;
            return (SingleVotePoll)await MapToCommon(model);
        }

        public async Task<IEnumerable<SingleVotePoll>> GetAllAsync()
        {
            var pollModels = await GetBaseQuery().ToListAsync();
            var tasks = pollModels.Select(async model => (SingleVotePoll)await MapToCommon(model));
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<SingleVotePoll>> GetPagedAsync(int page, int amount)
        {
            var pollModels = await GetBaseQuery()
                .Skip((page - 1) * amount)
                .Take(amount)
                .ToListAsync();
            
            var tasks = pollModels.Select(async model => (SingleVotePoll)await MapToCommon(model));
            return await Task.WhenAll(tasks);
        }

        public async Task AddAsync(SingleVotePoll entity)
        {
            var pollModel = await MapToModel(entity, null);
            await _dbSet.AddAsync((SingleVotePollModel)pollModel);
        }

        public async Task UpdateAsync(SingleVotePoll entity)
        {
            var model = await _dbSet
                .Include(p => p.Options)
                .Include(p => p.Status)
                .Include(p => p.Iteration).ThenInclude(i => i.Votes) 
                .FirstOrDefaultAsync(p => p.Id == entity.Id);

            if (model != null)
            {
                await MapToModel(entity, model);
            }
        }

        public async Task DeleteAsync(SingleVotePoll entity)
        {
            var model = await _dbSet.FindAsync(entity.Id);
            if (model != null)
            {
                _dbSet.Remove(model);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        private IQueryable<SingleVotePollModel> GetBaseQuery()
        {
            return _dbSet
                .AsNoTracking()
                .Include(p => p.Options)
                .Include(p => p.Status)
                .Include(p => p.Iteration)
                    .ThenInclude(i => i.Votes)
                    .ThenInclude(v => v.Person) 
                .AsSplitQuery();
        }

        private async Task<Poll> MapToCommon(SingleVotePollModel model)
        {
            var options = model.Options.Select(optModel => 
                new Option(optModel.Id, optModel.Name) 
            ).ToHashSet();
            
            var prevResult = new Dictionary<Guid, int>();
            var votes = new Dictionary<Person, Guid>();

            if (model.Iteration != null && model.Iteration.Votes.Any())
            {
                if (model.Status.IsOngoing)
                {
                    foreach (var voteModel in model.Iteration.Votes)
                    {
                        var person = await _personRepository.GetByIdAsync(voteModel.PersonId);
                        if (person != null) votes[person] = voteModel.OptionId;
                    }
                }
                else
                {
                    prevResult = model.Iteration.Votes
                        .ToDictionary(v => v.OptionId, v => v.weight);
                }
            }

            var status = false;
            if (model.Status != null)
            {
                status = model.Status.IsOngoing;
            }

            return new SingleVotePoll(
                model.Id,
                model.Title,
                options,
                model.Status.IsOngoing,
                prevResult,
                votes
            );
        }

        private async Task<PollModel> MapToModel(SingleVotePoll entity, SingleVotePollModel existingModel)
        {
            SingleVotePollModel model = existingModel;
            if (model == null)
            {
                model = new SingleVotePollModel { Id = entity.Id };
            }

            model.Status = new PollStatusModel();
            model.Status.IsOngoing = entity.IsOngoing;
            model.Title = entity.Title;
            
            var options = entity.GetOptions();
            var optionIds = options.Select(o => o.Id).ToList();
            model.Options = await _context.Options.Where(o => optionIds.Contains(o.Id)).ToListAsync();

            if (model.Iteration == null)
                model.Iteration = new IterationModel { Id = Guid.NewGuid(), PollId = model.Id };
            else
            {
                _context.Votes.RemoveRange(model.Iteration.Votes);
                model.Iteration.Votes.Clear(); 
            }

            if (entity.IsOngoing)
            {
                var votesDict = entity.GetVotes();
                foreach (var vote in votesDict)
                {
                    model.Iteration.Votes.Add(new VoteModel
                    {
                        Id = Guid.NewGuid(), PollId = model.Id, IterationId = model.Iteration.Id,
                        PersonId = vote.Key.Id,
                        OptionId = vote.Value,
                        weight = 1
                    });
                }
            }
            else
            {
                var prevResultDict = entity.PrevResult();
                if (prevResultDict.Any())
                {
                    foreach (var resultEntry in prevResultDict)
                    {
                        model.Iteration.Votes.Add(new VoteModel
                        {
                            Id = Guid.NewGuid(), PollId = model.Id, IterationId = model.Iteration.Id,
                            OptionId = resultEntry.Key,
                            weight = resultEntry.Value,
                            PersonId = Guid.Empty 
                        });
                    }
                }
            }
            
            return model;
        }
    }
}
