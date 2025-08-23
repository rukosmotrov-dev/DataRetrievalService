using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Infrastructure.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DataRetrievalService.Tests.Infrastructure;

public class StorageFactoryTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<StorageFactory>> _mockLogger;
    private readonly Mock<IOptions<StorageSettings>> _mockOptions;
    private readonly StorageSettings _storageSettings;

    public StorageFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<StorageFactory>>();
        _mockOptions = new Mock<IOptions<StorageSettings>>();
        _storageSettings = new StorageSettings();
        _mockOptions.Setup(x => x.Value).Returns(_storageSettings);
    }

    [Fact]
    public void Constructor_WhenNoStorageServicesConfigured_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyStorageServices = new List<IStorageService>();
        
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(emptyStorageServices);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object));
        
        Assert.Equal("No storage services are configured. At least one storage service must be registered in the dependency injection container.", exception.Message);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No storage services are configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenStorageServicesConfigured_RegistersAllStorages()
    {
        // Arrange
        var mockStorage1 = CreateMockStorageService("FileStorage", "File Storage", 1);
        var mockStorage2 = CreateMockStorageService("DatabaseStorage", "MSSQL Database", 2);
        
        var storageServices = new List<IStorageService> { mockStorage1.Object, mockStorage2.Object };
        
        _storageSettings.Storages.AddRange(new[]
        {
            new StorageConfiguration { Name = "File Storage", Type = "File Storage", Priority = 1 },
            new StorageConfiguration { Name = "Database Storage", Type = "MSSQL Database", Priority = 2 }
        });

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(storageServices);

        // Act
        var factory = new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object);

        // Assert
        var allStorages = factory.GetAllStorages().ToList();
        Assert.Equal(2, allStorages.Count);
        Assert.Contains(mockStorage1.Object, allStorages);
        Assert.Contains(mockStorage2.Object, allStorages);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Storage registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public void Constructor_WhenStorageServiceHasNoMatchingConfiguration_DoesNotRegisterStorage()
    {
        // Arrange
        var mockStorage = CreateMockStorageService("UnknownStorage", "Unknown Storage", 1);
        var storageServices = new List<IStorageService> { mockStorage.Object };
        
        // No matching configuration in _storageSettings

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(storageServices);

        // Act
        var factory = new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object);

        // Assert
        var allStorages = factory.GetAllStorages().ToList();
        Assert.Empty(allStorages);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Storage registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_WhenStorageSettingsIsNull_UsesDefaultEmptySettings()
    {
        // Arrange
        var mockStorage = CreateMockStorageService("FileStorage", "File Storage", 1);
        var storageServices = new List<IStorageService> { mockStorage.Object };
        
        _mockOptions.Setup(x => x.Value).Returns((StorageSettings)null!);

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(storageServices);

        // Act
        var factory = new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object);

        // Assert
        var allStorages = factory.GetAllStorages().ToList();
        Assert.Empty(allStorages); // No configuration means no storage gets registered
    }

    [Fact]
    public void Constructor_WhenMultipleStoragesWithSameType_RegistersLastOne()
    {
        // Arrange
        var mockStorage1 = CreateMockStorageService("FileStorage1", "File Storage", 1);
        var mockStorage2 = CreateMockStorageService("FileStorage2", "File Storage", 2);
        
        var storageServices = new List<IStorageService> { mockStorage1.Object, mockStorage2.Object };
        
        _storageSettings.Storages.AddRange(new[]
        {
            new StorageConfiguration { Name = "File Storage 1", Type = "File Storage", Priority = 1 },
            new StorageConfiguration { Name = "File Storage 2", Type = "File Storage", Priority = 2 }
        });

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(storageServices);

        // Act
        var factory = new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object);

        // Assert
        var allStorages = factory.GetAllStorages().ToList();
        Assert.Single(allStorages);
        Assert.Equal(mockStorage2.Object, allStorages.First()); // Last one should be registered
    }

    [Fact]
    public void Constructor_WhenStorageServicesConfigured_LogsCorrectInformation()
    {
        // Arrange
        var mockStorage = CreateMockStorageService("FileStorage", "File Storage", 1);
        var storageServices = new List<IStorageService> { mockStorage.Object };
        
        _storageSettings.Storages.Add(new StorageConfiguration 
        { 
            Name = "File Storage", 
            Type = "File Storage", 
            Priority = 1 
        });

        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IEnumerable<IStorageService>)))
            .Returns(storageServices);

        // Act
        var factory = new StorageFactory(_mockServiceProvider.Object, _mockLogger.Object, _mockOptions.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Storage registered: File Storage (File Storage) with priority 1.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Mock<IStorageService> CreateMockStorageService(string name, string type, int priority)
    {
        var mock = new Mock<IStorageService>();
        mock.Setup(x => x.StorageName).Returns(name);
        mock.Setup(x => x.StorageType).Returns(type);
        mock.Setup(x => x.Priority).Returns(priority);
        return mock;
    }
}
