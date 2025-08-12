using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataRetrievalService.Infrastructure.Repositories
{
    public class DataRepository : IDataRepository
    {
        private readonly AppDbContext _dbContext;
        public DataRepository(AppDbContext db) => _dbContext = db;

        public async Task<DataItem?> GetByIdAsync(Guid id) => 
            await _dbContext.DataItems.FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(DataItem item)
        {
            _dbContext.DataItems.Add(item);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(DataItem item)
        {
            _dbContext.DataItems.Update(item);
            await _dbContext.SaveChangesAsync();
        }
    }
}
