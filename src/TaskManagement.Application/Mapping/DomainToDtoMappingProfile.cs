using AutoMapper;
using TaskManagement.Application.DTOs.Responses;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mapping;

public class DomainToDtoMappingProfile : Profile
{
    public DomainToDtoMappingProfile()
    {
        CreateMap<TaskItem, TaskDto>();
    }
}
