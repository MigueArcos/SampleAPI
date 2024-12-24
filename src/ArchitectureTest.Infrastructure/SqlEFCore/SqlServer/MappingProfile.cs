using AutoMapper;

using DomainEntities = ArchitectureTest.Domain.Entities;
using DatabaseEntities = ArchitectureTest.Databases.SqlServer.Entities;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlServerMappingProfile : Profile {
    public SqlServerMappingProfile()
    {
        CreateMap<DomainEntities.Note, DatabaseEntities.Note>().ReverseMap();

        CreateMap<DomainEntities.Checklist, DatabaseEntities.Checklist>();
        CreateMap<DatabaseEntities.Checklist, DomainEntities.Checklist>()
            .ForMember(e => e.Details, o => o.MapFrom(src => src.ChecklistDetails))
            .AfterMap((src, dest, context) => {
                dest.Details = DomainEntities.Checklist.FormatChecklistDetails(dest.Details);
            });

        CreateMap<DomainEntities.ChecklistDetail, DatabaseEntities.ChecklistDetail>().ReverseMap();
        CreateMap<DomainEntities.User, DatabaseEntities.User>().ReverseMap();
        CreateMap<DomainEntities.UserToken, DatabaseEntities.UserToken>().ReverseMap();
    }
}
