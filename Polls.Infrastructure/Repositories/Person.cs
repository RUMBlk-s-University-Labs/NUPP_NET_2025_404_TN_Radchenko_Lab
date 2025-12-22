using Microsoft.EntityFrameworkCore;
using Polls.Common;
using Polls.Infrastructure.Models;

namespace Polls.Infrastructure.Repositories
{
    public class PersonRepository : IRepository<Person>
    {
        private readonly PollsContext _context;
        private readonly DbSet<PersonModel> _dbSet;

        public PersonRepository(PollsContext context)
        {
            _context = context;
            _dbSet = _context.Set<PersonModel>();
        }

        public async Task<Person> GetByIdAsync(Guid id)
        {
            var personModel = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (personModel == null) return null;

            return new Person(personModel.Id, personModel.Name);
        }


        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            var personModels = await _dbSet
                .AsNoTracking()
                .ToListAsync();

            return personModels.Select(model =>
                new Person(model.Id, model.Name)
            );
        }
        
        public async Task<IEnumerable<Person>> GetPagedAsync(int page, int amount)
        {
            var personModels = await _dbSet.AsNoTracking()
                                    .Skip((page - 1) * amount)
                                    .Take(amount)
                                    .ToListAsync();
            
            return personModels.Select(model =>
                new Person(model.Id, model.Name)
            );
        }

        public async Task AddAsync(Person entity)
        {
            var personModel = new PersonModel
            {
                Id = entity.Id,
                Name = entity.Name
            };

            await _dbSet.AddAsync(personModel);
        }

        public async Task DeleteAsync(Person entity)
        {
            var personModel = await _dbSet.FindAsync(entity.Id);
            if (personModel != null)
            {
                _dbSet.Remove(personModel);
            }
        }

        public async Task UpdateAsync(Person entity)
        {
            var personModel = await _dbSet
                .FirstOrDefaultAsync(p => p.Id == entity.Id);

            if (personModel != null)
            {
                personModel.Name = entity.Name;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

