using FinancialChat.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FinancialChat.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly FinancialChatDbContext _context;

        public GenericRepository(FinancialChatDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<T> CreateAsync(T model)
        {
            if (model is null)
            {
                throw new ArgumentException("Model must not be null", nameof(model));
            }

            EntityEntry<T> result = await _context.Set<T>().AddAsync(model);

            await _context.SaveChangesAsync();

            return result.Entity;
        }
    }
}
