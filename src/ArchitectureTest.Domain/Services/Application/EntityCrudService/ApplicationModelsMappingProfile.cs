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

    public static List<ChecklistDetail> FormatChecklistDetails(
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
        return selection ?? [];
    }

    public static List<string> FindAllDetailsToRemove(
        ICollection<ChecklistDetail>? flattenedDetails, List<string>? detailsToRemove
    ){
        List<string> findDetailsToRemoveRecursive(List<string>? currentIDsToRemove){
            List<string> result = [];

            currentIDsToRemove?.ForEach(id => {
                result.Add(id);
                var childDetails = flattenedDetails?.Where(d => d.ParentDetailId == id).Select(d => d.Id).ToList() ?? [];
                if (childDetails.Count > 0)
                    result.AddRange(findDetailsToRemoveRecursive(childDetails));
            });

            return result;
        }

        return findDetailsToRemoveRecursive(detailsToRemove).Distinct().ToList();
    }

    public static List<ChecklistDetail> FlattenAndPopulateChecklistDetails(
        string checklistId, IList<ChecklistDetail>? details, string? parentDetailId = null
    ) {
        var flattenedDetails = new List<ChecklistDetail>();
        if (details == null)
            return flattenedDetails;

        for(int i = 0; i < details.Count; i++)
        {
            var detail = details[i];
            var subItems = detail.SubItems;

            var newDetail = new ChecklistDetail {
                Id = Guid.CreateVersion7().ToString("N"),
                Status = detail.Status,
                TaskName = detail.TaskName,
                ParentDetailId = parentDetailId,
                ChecklistId = checklistId,
                CreationDate = DateTime.Now,
                ModificationDate = null,
                SubItems = null
            };

            flattenedDetails.Add(newDetail);

            if (subItems != null && subItems.Count > 0)
            {
                // detail.SubItems = FlattenChecklistDetails(parentChecklistId, detail.SubItems, detail.Id);
                flattenedDetails.AddRange(FlattenAndPopulateChecklistDetails(checklistId, subItems, newDetail.Id));
            }
        }
        return flattenedDetails;
    }

    public static List<ChecklistDetail> FlattenChecklistDetails(IList<ChecklistDetail>? details) {
        var flattenedDetails = new List<ChecklistDetail>();
        if (details == null)
            return flattenedDetails;

        for(int i = 0; i < details.Count; i++)
        {
            var detail = details[i];
            var subItems = detail.SubItems;
            
            var newDetail = new ChecklistDetail {
                Id = detail.Id,
                Status = detail.Status,
                TaskName = detail.TaskName,
                ParentDetailId = detail.ParentDetailId,
                ChecklistId = detail.ChecklistId,
                CreationDate = DateTime.Now,
                ModificationDate = null,
                SubItems = null
            };

            flattenedDetails.Add(newDetail);

            if (subItems != null && subItems.Count > 0)
            {
                // detail.SubItems = FlattenChecklistDetails(parentChecklistId, detail.SubItems, detail.Id);
                flattenedDetails.AddRange(FlattenChecklistDetails(subItems));
            }
        }
        return flattenedDetails;
    }
}
