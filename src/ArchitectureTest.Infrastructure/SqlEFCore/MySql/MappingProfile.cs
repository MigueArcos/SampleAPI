using ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Domain.Entities;
using AutoMapper;

namespace ArchitectureTest.Infrastructure.SqlEFCore.MySql;

public class MySqlMappingProfile : Profile {
    public MySqlMappingProfile()
    {
        CreateMap<NoteEntity, Note>().ReverseMap();

        CreateMap<Checklist, ChecklistEntity>()
            .ForMember(e => e.Details, o => o.MapFrom(src => src.ChecklistDetails))
            .AfterMap((src, dest, context) => {
                dest.Details = ChecklistEntity.FormatChecklistDetails(dest.Details);
            });
        CreateMap<ChecklistEntity, Checklist>();

        CreateMap<ChecklistDetailEntity, ChecklistDetail>().ReverseMap();

        CreateMap<UserEntity, User>().ReverseMap();
        CreateMap<UserTokenEntity, UserToken>().ReverseMap();
    }
}
