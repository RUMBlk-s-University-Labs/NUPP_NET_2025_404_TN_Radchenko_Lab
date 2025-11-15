using Microsoft.EntityFrameworkCore;
using Polls.Common;
using Polls.Infrastructure.Models;

namespace Polls.Infrastructure.Repositories
{
    public class PollRepository : IRepository<Poll>
    {
        private readonly PollsContext _context;
        private readonly DbSet<PollModel> _dbSet;
        
        private readonly PersonRepository _personRepository;

        public PollRepository(PollsContext context, PersonRepository personRepository)
        {
            _context = context;
            _dbSet = context.Set<PollModel>();
            _personRepository = personRepository;
        }

        public async Task<Poll> GetByIdAsync(Guid id)
        {
            var model = await GetBaseQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (model == null) return null;
            
            return await MapToCommon(model);
        }

        public async Task<IEnumerable<Poll>> GetAllAsync()
        {
            var pollModels = await GetBaseQuery().ToListAsync();
            var tasks = pollModels.Select(MapToCommon).ToList();
            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<Poll>> GetPagedAsync(int page, int amount)
        {
            var pollModels = await GetBaseQuery()
                .Skip((page - 1) * amount)
                .Take(amount)
                .ToListAsync();
            
            var tasks = pollModels.Select(MapToCommon).ToList();
            return await Task.WhenAll(tasks);
        }

        public async Task AddAsync(Poll entity)
        {
            if (entity.GetType() != typeof(Poll))
            {
                throw new ArgumentException("PollRepository може додавати лише базові Poll об'єкти.");
            }
            var pollModel = await MapToModel(entity, null);
            await _dbSet.AddAsync(pollModel);
        }

        public async Task UpdateAsync(Poll entity)
        {
            var model = await _dbSet
                .Include(p => p.Options)
                .Include(p => p.Iteration).ThenInclude(i => i.Votes) 
                .FirstOrDefaultAsync(p => p.Id == entity.Id);

            if (model != null)
            {
                await MapToModel(entity, model);
            }
        }

        public async Task DeleteAsync(Poll entity)
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

        private IQueryable<PollModel> GetBaseQuery()
        {
            return _dbSet
                .Where(p => !(p is SingleVotePollModel) && !(p is RankedPollModel))
                .Include(p => p.Options)
                .Include(p => p.Iteration)
                    .ThenInclude(i => i.Votes)
                    .ThenInclude(v => v.Person) 
                .AsSplitQuery();
        }

        private async Task<Poll> MapToCommon(PollModel model)
        {
            var options = model.Options.Select(optModel => 
                new Option(optModel.Id, optModel.Name) 
            ).ToHashSet();
            
            var prevResult = new Dictionary<Guid, int>();

            if (model.Iteration != null && model.Iteration.Votes.Any())
            {
                if (!model.Status.IsOngoing)
                {
                    prevResult = model.Iteration.Votes
                        .ToDictionary(v => v.OptionId, v => v.weight);
                }
            }

            return new Poll(
                model.Id, 
                model.Title,
                options, 
                model.Status.IsOngoing,
                prevResult
            );
        }

        private async Task<PollModel> MapToModel(Poll entity, PollModel existingModel)
        {
            PollModel model = existingModel;
            if (model == null)
            {
                model = new PollModel { Id = entity.Id };
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

            if (!entity.IsOngoing)
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

