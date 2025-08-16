using DataRetrievalService.Domain.Enums;

namespace DataRetrievalService.Application.Interfaces;

public interface IStorageFactory
{
    IStorageService GetStorage(StorageType storageType);
}
