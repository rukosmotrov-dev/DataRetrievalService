using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Entities;
using DataRetrievalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DataRetrievalService.Infrastructure.Storage.Services;

public sealed class DataRepository(AppDbContext dbContext) : IDataRepository
{
    public Task<DataItem?> GetByIdAsync(Guid id) => 
        dbContext.DataItems.FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync(DataItem item)
    {
        dbContext.DataItems.Add(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(DataItem item)
    {
        dbContext.DataItems.Update(item);
        await dbContext.SaveChangesAsync();
    }
}
