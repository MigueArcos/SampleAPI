using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface IChecklistCrudService : ICrudService<Checklist, ChecklistDTO> {
    Task<Result<IList<ChecklistDTO>, AppError>> GetUserChecklists();
}
