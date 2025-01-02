using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Models;
using AutoMapper;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService;

public class ApplicationModelsMappingProfile : Profile 
{
    public ApplicationModelsMappingProfile()
    {
        CreateMap<Note, NoteDTO>().ReverseMap();
        CreateMap<Checklist, ChecklistDTO>().ReverseMap();
        CreateMap<ChecklistDetail, ChecklistDetailDTO>().ReverseMap();
    }
}
