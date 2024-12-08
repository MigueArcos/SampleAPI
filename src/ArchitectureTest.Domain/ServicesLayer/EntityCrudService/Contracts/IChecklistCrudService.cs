using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;

public interface IChecklistCrudService : ICrudService<Checklist, ChecklistDTO> {
    Task<IList<ChecklistDTO>> GetUserChecklists();
}
