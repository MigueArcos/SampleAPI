using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.Repositories.BasicRepo;

namespace ArchitectureTest.Data.UnitOfWork {
	public interface IUnitOfWork {
		IRepository<Note> NotesRepository { get; }
		IRepository<Checklist> ChecklistRepository { get; }
		IRepository<ChecklistDetail> ChecklistDetailRepository { get; }
		void StartTransaction();
		void Commit();
		void Rollback();
	}
}
