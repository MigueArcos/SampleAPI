using ArchitectureTest.Databases.SqlServer.Entities;
using ArchitectureTest.Domain.Errors;
using ArchitectureTest.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.Services.Application.EntityCrudService.Contracts;

public interface INotesCrudService : ICrudService<Note, NoteDTO> {
    Task<Result<IList<NoteDTO>, AppError>> GetUserNotes();
}
