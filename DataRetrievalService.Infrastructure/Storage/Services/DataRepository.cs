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
        var existingEntity = dbContext.DataItems.Local.FirstOrDefault(x => x.Id == item.Id);
        
        if (existingEntity != null)
        {
            dbContext.Entry(existingEntity).CurrentValues.SetValues(item);
        }
        else
        {
            dbContext.DataItems.Update(item);
        }
        
        await dbContext.SaveChangesAsync();
    }
}
