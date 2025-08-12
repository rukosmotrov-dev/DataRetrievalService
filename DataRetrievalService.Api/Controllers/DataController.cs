using AutoMapper;
using DataRetrievalService.Api.Contracts.Data;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataRetrievalService.Api.Controllers
{
    [Route("data")]
    [ApiController]
    public class DataController(IDataRetrievalService service, IMapper mapper) : ControllerBase
    {
        private readonly IDataRetrievalService _service = service;
        private readonly IMapper _mapper = mapper;
        private const string UserAndAdminRoles = $"{nameof(UserRole.Admin)},{nameof(UserRole.User)}";
        private const string AdminRole = $"{nameof(UserRole.Admin)}";

        [HttpGet("{id:guid}")]
        [Authorize(Roles = UserAndAdminRoles)]
        public async Task<IActionResult> Get(Guid id)
        {
            var dto = await _service.GetAsync(id);
            return dto is null ? NotFound() : Ok(_mapper.Map<DataItemResponse>(dto));
        }

        [HttpPost]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> Create([FromBody] CreateDataItemRequest req)
        {
            var createdData = await service.CreateAsync(new CreateDataItemDto { Value = req.Value });
            var response = _mapper.Map<DataItemResponse>(createdData);

            return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = AdminRole)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDataItemRequest request)
        {
            await service.UpdateAsync(id, _mapper.Map<UpdateDataItemDto>(request));

            return NoContent();
        }
    }
}
