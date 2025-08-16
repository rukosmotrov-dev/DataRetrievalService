using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Application.Services;
using DataRetrievalService.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DataRetrievalService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDataRetrievalService, Services.DataRetrievalService>();

        services.Decorate<IDataRetrievalService, LoggingDataRetrievalServiceDecorator>();

        services.AddValidatorsFromAssembly(typeof(CreateDataItemValidator).Assembly);
        return services;
    }
}
