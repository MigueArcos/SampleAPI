using System;
using System.Collections.Generic;
using System.Linq;
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

    public static List<ChecklistDetail>? FormatChecklistDetails(
        ICollection<ChecklistDetail>? flattenedDetails, string? parentDetailId = null
    ){
        var selection = flattenedDetails?.Where(d => d.ParentDetailId == parentDetailId).Select(cD => new ChecklistDetail {
            Id = cD.Id,
            ChecklistId = cD.ChecklistId,
            ParentDetailId = cD.ParentDetailId,
            TaskName = cD.TaskName,
            Status = cD.Status,
            CreationDate = cD.CreationDate,
            ModificationDate = cD.ModificationDate
        }).ToList();
        selection?.ForEach(i => {
            i.SubItems = FormatChecklistDetails(flattenedDetails, i.Id);
        });
        return selection;
    }

    public static List<ChecklistDetailDTO> FlattenChecklistDetails(
        string parentChecklistId, IList<ChecklistDetailDTO>? details, string? parentDetailId = null
    ) {
        var flattenedDetails = new List<ChecklistDetailDTO>();
        if (details == null)
            return flattenedDetails;

        for(int i = 0; i < details.Count; i++)
        {
            var detail = details[i];
            var subItems = detail.SubItems;
            detail = detail with { 
                Id = Guid.CreateVersion7().ToString("N"),
                ParentDetailId = parentDetailId,
                ChecklistId = parentChecklistId,
                CreationDate = DateTime.Now,
                ModificationDate = null,
                SubItems = null
            };

            flattenedDetails.Add(detail);

            if (subItems != null && subItems.Count > 0)
            {
                // detail.SubItems = FlattenChecklistDetails(parentChecklistId, detail.SubItems, detail.Id);
                flattenedDetails.AddRange(FlattenChecklistDetails(parentChecklistId, subItems, detail.Id));
            }
        }
        return flattenedDetails;
    }
}
