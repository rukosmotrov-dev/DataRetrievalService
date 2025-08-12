using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DataRetrievalService.Application.Services
{
    public sealed class LoggingDataRetrievalServiceDecorator : IDataRetrievalService
    {
        private readonly IDataRetrievalService _inner;
        private readonly ILogger<LoggingDataRetrievalServiceDecorator> _logger;

        public LoggingDataRetrievalServiceDecorator(
            IDataRetrievalService inner,
            ILogger<LoggingDataRetrievalServiceDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<DataItemDto?> GetAsync(Guid id)
        {
            _logger.LogInformation($"{nameof(GetAsync)} starting for Id = {id} .");
            var sw = Stopwatch.StartNew();
            try
            {
                var result = await _inner.GetAsync(id);
                _logger.LogInformation($"{nameof(GetAsync)} finished for Id = {id} hit = {result is not null} in {sw.ElapsedMilliseconds} ms.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAsync)} failed for Id={id} after {sw.ElapsedMilliseconds} ms.");
                throw;
            }
        }

        public async Task<DataItemDto> CreateAsync(CreateDataItemDto dto)
        {
            _logger.LogInformation($"{nameof(GetAsync)} starting.");
            var sw = Stopwatch.StartNew();
            try
            {
                var created = await _inner.CreateAsync(dto);
                _logger.LogInformation($"{nameof(GetAsync)} finished Id = {created.Id} in {sw.ElapsedMilliseconds} ms.");
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAsync)} failed after {sw.ElapsedMilliseconds} ms.");
                throw;
            }
        }

        public async Task UpdateAsync(Guid id, UpdateDataItemDto dto)
        {
            _logger.LogInformation($"{nameof(UpdateAsync)} starting for Id = {id} .");
            var sw = Stopwatch.StartNew();
            try
            {
                await _inner.UpdateAsync(id, dto);
                _logger.LogInformation($"{nameof(UpdateAsync)} finished for Id = {id} in {sw.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateAsync)} failed for Id = {id} after {sw.ElapsedMilliseconds} ms.");
                throw;
            }
        }
    }
}
