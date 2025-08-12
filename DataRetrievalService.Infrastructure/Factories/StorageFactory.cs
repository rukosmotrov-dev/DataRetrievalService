using DataRetrievalService.Application.Interfaces;

namespace DataRetrievalService.Infrastructure.Factories
{
    public class StorageFactory : IStorageFactory
    {
        private readonly ICacheService _cache;
        private readonly IFileStorageService _file;
        private readonly IDataRepository _repo;

        public StorageFactory(ICacheService cache, IFileStorageService file, IDataRepository repo)
        {
            _cache = cache; 
            _file = file; 
            _repo = repo;
        }

        public ICacheService Cache() => _cache;
        public IFileStorageService File() => _file;
        public IDataRepository Database() => _repo;
    }
}
