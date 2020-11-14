using ArchitectureTest.Data.Database.MySQL.Entities;
using ArchitectureTest.Domain.Repositories.BasicRepo;

namespace ArchitectureTest.Domain.UnitOfWork {
	public interface IUnitOfWork {
		IRepository<Note> NotesRepository { get; }
		IRepository<Checklist> ChecklistRepository { get; }
		IRepository<ChecklistDetail> ChecklistDetailRepository { get; }
		IRepository<TEntity> Repository<TEntity>() where TEntity : Entity;
		void StartTransaction();
		void Commit();
		void Rollback();
	}
}
