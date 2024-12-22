using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;

public interface IChecklistCrudService : ICrudService<ChecklistEntity> {
    Task<Result<IList<ChecklistEntity>, AppError>> GetUserChecklists();
}
