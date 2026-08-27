using AutoMapper;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mapping;

public class RequestToDomainMappingProfile : Profile
{
    public RequestToDomainMappingProfile()
    {
        CreateMap<CreateTaskRequest, TaskItem>();
        CreateMap<UpdateTaskRequest, TaskItem>();
    }
}
