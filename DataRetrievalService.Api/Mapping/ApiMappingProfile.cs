using AutoMapper;
using DataRetrievalService.Api.Contracts.Data;
using DataRetrievalService.Application.DTOs;

namespace DataRetrievalService.Api.Mapping
{
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            CreateMap<DataItemDto, DataItemResponse>();

            CreateMap<CreateDataItemRequest, CreateDataItemDto>();
            CreateMap<UpdateDataItemRequest, UpdateDataItemDto>();
        }
    }
}
