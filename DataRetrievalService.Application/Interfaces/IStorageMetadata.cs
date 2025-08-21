namespace DataRetrievalService.Application.Interfaces;

public interface IStorageMetadata
{
    string StorageType { get; }
    int Priority { get; }
}
