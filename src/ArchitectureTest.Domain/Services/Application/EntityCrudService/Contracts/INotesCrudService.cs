using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface INotesCrudService : ICrudService<Note> {
    Task<Result<IList<Note>, AppError>> GetUserNotes();
}
