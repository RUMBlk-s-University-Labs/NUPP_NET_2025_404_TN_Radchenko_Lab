using Microsoft.EntityFrameworkCore;
using Polls.Common;
using Polls.Infrastructure.Models;

namespace Polls.Infrastructure.Repositories
{
    public class OptionRepository : IRepository<Option>
    {
        private readonly PollsContext _context;
        private readonly DbSet<OptionModel> _dbSet;

        public OptionRepository(PollsContext context)
        {
            _context = context;
            _dbSet = _context.Set<OptionModel>();
        }

        public async Task<Option> GetByIdAsync(Guid id)
        {
            var OptionModel = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (OptionModel == null) return null;

            return new Option(OptionModel.Id, OptionModel.Name);
        }


        public async Task<IEnumerable<Option>> GetAllAsync()
        {
            var OptionModels = await _dbSet
                .AsNoTracking()
                .ToListAsync();

            return OptionModels.Select(model =>
                new Option(model.Id, model.Name)
            );
        }
        
        public async Task<IEnumerable<Option>> GetPagedAsync(int page, int amount)
        {
            var OptionModels = await _dbSet.AsNoTracking()
                                    .Skip((page - 1) * amount)
                                    .Take(amount)
                                    .ToListAsync();
            
            return OptionModels.Select(model =>
                new Option(model.Id, model.Name)
            );
        }

        public async Task AddAsync(Option entity)
        {
            var OptionModel = new OptionModel
            {
                Id = entity.Id,
                Name = entity.Name
            };

            await _dbSet.AddAsync(OptionModel);
        }

        public async Task DeleteAsync(Option entity)
        {
            var OptionModel = await _dbSet.FindAsync(entity.Id);
            if (OptionModel != null)
            {
                _dbSet.Remove(OptionModel);
            }
        }

        public async Task UpdateAsync(Option entity)
        {
            var OptionModel = await _dbSet
                .FirstOrDefaultAsync(p => p.Id == entity.Id);

            if (OptionModel != null)
            {
                OptionModel.Name = entity.Name;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

