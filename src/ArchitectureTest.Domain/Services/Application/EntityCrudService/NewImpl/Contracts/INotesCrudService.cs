using ArchitectureTest.Domain.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.NewImpl.Contracts;

public interface INotesCrudService : ICrudService<NoteEntity> {
    Task<Result<IList<NoteEntity>, AppError>> GetUserNotes();
}
