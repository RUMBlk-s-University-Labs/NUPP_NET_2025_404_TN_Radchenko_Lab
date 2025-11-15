using Microsoft.EntityFrameworkCore;
using Polls.Common;
using Polls.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polls.Infrastructure.Repositories
{
    public class RankedPollRepository : IRepository<RankedPoll>
    {
        private readonly PollsContext _context;
        private readonly DbSet<RankedPollModel> _dbSet;
        private readonly PersonRepository _personRepository;

        public RankedPollRepository(PollsContext context, PersonRepository personRepository)
        {
            _context = context;
            _dbSet = context.Set<RankedPollModel>();
            _personRepository = personRepository;
        }

        public async Task<RankedPoll> GetByIdAsync(Guid id)
        {
            var model = await GetBaseQuery()
                .Include(p => p.Options)
                .Include(p => p.Status)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (model == null) return null;
            return (RankedPoll)await MapToCommon(model);
        }

        public async Task<IEnumerable<RankedPoll>> GetAllAsync()
        {
            var pollModels = await GetBaseQuery().Include(p => p.Options).Include(p => p.Status).ToListAsync();
            var tasks = pollModels.Select(async model => (RankedPoll) await MapToCommon(model));
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<RankedPoll>> GetPagedAsync(int page, int amount)
        {
            var pollModels = await GetBaseQuery()
                .Include(p => p.Status)
                .Include(p => p.Options)
                .Skip((page - 1) * amount)
                .Take(amount)
                .ToListAsync();
            
            var tasks = pollModels.Select(async model => (RankedPoll)await MapToCommon(model));
            return await Task.WhenAll(tasks);
        }

        public async Task AddAsync(RankedPoll entity)
        {
            var pollModel = await MapToModel(entity, null);
            await _dbSet.AddAsync((RankedPollModel)pollModel);
        }

        public async Task UpdateAsync(RankedPoll entity)
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

        public async Task DeleteAsync(RankedPoll entity)
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

        private IQueryable<RankedPollModel> GetBaseQuery()
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

        private async Task<Poll> MapToCommon(RankedPollModel model)
        {
            var options = model.Options.Select(optModel => 
                new Option(optModel.Id, optModel.Name) 
            ).ToHashSet();
            
            var prevResult = new Dictionary<Guid, int>();
            var votes = new Dictionary<Person, Dictionary<Guid, int>>();

            if (model.Iteration != null && model.Iteration.Votes.Any())
            {
                if (model.Status.IsOngoing)
                {
                    var groups = model.Iteration.Votes.GroupBy(v => v.PersonId);
                    foreach (var group in groups)
                    {
                        var person = await _personRepository.GetByIdAsync(group.Key);
                        if (person != null)
                        {
                            votes[person] = group.ToDictionary(v => v.OptionId, v => v.weight);
                        }
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

            return new RankedPoll(
                model.Id, 
                model.Title, 
                options, 
                status, 
                prevResult,
                votes
            );
        }

        private async Task<PollModel> MapToModel(RankedPoll entity, RankedPollModel existingModel)
        {
            RankedPollModel model = existingModel;
            if (model == null)
            {
                model = new RankedPollModel { Id = entity.Id };
            }

            model.Status ??= new PollStatusModel();
            model.Status.IsOngoing = entity.IsOngoing;
            Console.WriteLine(model.Status.IsOngoing);
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
                foreach (var personVotes in votesDict)
                {
                    foreach (var rank in personVotes.Value)
                    {
                        model.Iteration.Votes.Add(new VoteModel
                        {
                            Id = Guid.NewGuid(), PollId = model.Id, IterationId = model.Iteration.Id,
                            PersonId = personVotes.Key.Id,
                            OptionId = rank.Key,
                            weight = rank.Value
                        });
                    }
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
            Console.WriteLine(model.Status.IsOngoing);
            return model;
        }
    }
}
