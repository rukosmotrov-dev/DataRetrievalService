namespace DataRetrievalService.Application.Interfaces
{
    public interface IStorageFactory
    {
        ICacheService Cache();
        IFileStorageService File();
        IDataRepository Database();
    }
}
