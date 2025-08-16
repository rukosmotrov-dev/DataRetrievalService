using AutoMapper;
using DataRetrievalService.Application.DTOs;
using DataRetrievalService.Domain.Entities;

namespace DataRetrievalService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DataItem, DataItemDto>().ReverseMap();
        CreateMap<CreateDataItemDto, DataItem>().ReverseMap();
        CreateMap<UpdateDataItemDto, DataItem>().ReverseMap();
    }
}
