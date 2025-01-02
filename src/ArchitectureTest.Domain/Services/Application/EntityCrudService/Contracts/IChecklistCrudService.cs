using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

// It would have been great to use a single generic type parameter for ICrudService, and define the application models
// in the web layer, in a way that the web layer does the mapping in a ControllerBase with the 2 type arguments only in
// that layer, but that would create problems in the application layer when we want to use transactions because the app
// layer wouldn't know application input models (such as the model for Update Checklist), this could be solved maybe
// segregating the methods for Update in mini methods, but again this would be a problem when using transactions
public interface IChecklistCrudService : ICrudService<Checklist, ChecklistDTO> {
    Task<Result<IList<ChecklistDTO>, AppError>> GetUserChecklists();
}
