using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Services;
using DataRetrievalService.Application.Options;
using Microsoft.Extensions.DependencyInjection;

namespace DataRetrievalService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDataRetrievalService, DataRetrievalService.Application.Services.DataRetrievalService>();
        
        services.Configure<StorageSettings>(options => { });
        
        return services;
    }
}
