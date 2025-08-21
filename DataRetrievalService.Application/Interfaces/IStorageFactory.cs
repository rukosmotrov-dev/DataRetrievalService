using DataRetrievalService.Domain.Enums;

namespace DataRetrievalService.Application.Interfaces;

public interface IStorageFactory
{
    IEnumerable<IStorageService> GetAllStorages();
}
