using DomainEntities = ArchitectureTest.Domain.Entities;
using DatabaseEntities = ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Domain.Services.Application.EntityCrudService;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlMappingProfile : ApplicationModelsMappingProfile
{
    public MySqlMappingProfile() : base()
    {
        CreateMap<DomainEntities.Note, DatabaseEntities.Note>().ReverseMap();

        CreateMap<DomainEntities.Checklist, DatabaseEntities.Checklist>();
        CreateMap<DatabaseEntities.Checklist, DomainEntities.Checklist>()
            .ForMember(e => e.Details, o => o.MapFrom(src => src.ChecklistDetails))
            .AfterMap((src, dest, context) => {
                dest.Details = FormatChecklistDetails(dest.Details);
            });

        CreateMap<DomainEntities.ChecklistDetail, DatabaseEntities.ChecklistDetail>().ReverseMap();
        CreateMap<DomainEntities.User, DatabaseEntities.User>().ReverseMap();
        CreateMap<DomainEntities.UserToken, DatabaseEntities.UserToken>().ReverseMap();
    }
}
