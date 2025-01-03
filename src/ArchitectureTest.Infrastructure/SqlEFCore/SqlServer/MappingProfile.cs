using DomainEntities = ArchitectureTest.Domain.Entities;
using DatabaseEntities = ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;

namespace ArchitectureTest.Infrastructure.SqlEFCore.SqlServer;

public class SqlServerMappingProfile : ApplicationModelsMappingProfile
{
    public SqlServerMappingProfile() : base()
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
