using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.Models;
using ArchitectureTest.Domain.ServiceLayer.EntityCrudService.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Domain.ServicesLayer.EntityCrudService.Contracts;

public interface INotesCrudService : ICrudService<Note, NoteDTO> {
    Task<IList<NoteDTO>> GetUserNotes();
}
