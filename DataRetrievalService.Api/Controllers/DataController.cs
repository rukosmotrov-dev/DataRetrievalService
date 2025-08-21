using AutoMapper;
using DataRetrievalService.Api.Contracts.Data;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Application.Interfaces;
using DataRetrievalService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataRetrievalService.Api.Controllers;

[ApiController]
[Route("data")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class DataController(IDataRetrievalService service, IMapper mapper) : ControllerBase
{
    private readonly IDataRetrievalService _service = service;
    private readonly IMapper _mapper = mapper;
    private const string UserAndAdminRoles = $"{nameof(UserRole.Admin)},{nameof(UserRole.User)}";
    private const string AdminRole = $"{nameof(UserRole.Admin)}";

    [HttpGet("{id:guid}")]
    [Authorize(Roles = UserAndAdminRoles)]
    [ProducesResponseType(typeof(DataItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataItemResponse>> Get(Guid id)
    {
        var item = await _service.GetAsync(id);
        if (item is null)
            return NotFound();

        return Ok(_mapper.Map<DataItemResponse>(item));
    }

    [HttpPost]
    [Authorize(Roles = AdminRole)]
    [ProducesResponseType(typeof(DataItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataItemResponse>> Create([FromBody] CreateDataItemRequest request)
    {
        var dto = _mapper.Map<CreateDataItemDto>(request);
        var item = await _service.CreateAsync(dto);

        return CreatedAtAction(nameof(Get), new { id = item.Id },
            _mapper.Map<DataItemResponse>(item));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDataItemRequest request)
    {
        var item = await _service.GetAsync(id);
        if (item is null)
            return NotFound();
        
        var dto = _mapper.Map<UpdateDataItemDto>(request);
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }
}
