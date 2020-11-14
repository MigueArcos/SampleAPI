using ArchitectureTest.Data.Database.MySQL.Entities;
using ArchitectureTest.Domain.Repositories;
using ArchitectureTest.Domain.Repositories.BasicRepo;
using ArchitectureTest.Domain.UnitOfWork.RepoFactory;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.UnitOfWork {
	public class RepoDictionary<TEntity> : Dictionary<string, IRepository<TEntity>> where TEntity: Entity {

	}
	public class UnitOfWork : IUnitOfWork, IDisposable {
		private IRepository<Note> notesRepository;
		private IRepository<Checklist> checklistRepository;
		private IRepository<ChecklistDetail> checklistDetailRepository;
		private readonly DatabaseContext databaseContext;
		private IDbContextTransaction transaction;
		private IRepositoryFactory repositoryFactory;
		private Dictionary<string, object> repos = new Dictionary<string, object>();
		public UnitOfWork(DatabaseContext databaseContext) {
			this.databaseContext = databaseContext;
			repositoryFactory = new RepositoryFactory(databaseContext);
		}

		public IRepository<Note> NotesRepository {
			get {

				if (notesRepository == null) {
					notesRepository = new Repository<Note>(databaseContext);
				}
				return notesRepository;
			}
		}

		public IRepository<Checklist> ChecklistRepository {
			get {

				if (checklistRepository == null) {
					checklistRepository = new ChecklistRepository(databaseContext);
				}
				return checklistRepository;
			}
		}

		public IRepository<ChecklistDetail> ChecklistDetailRepository {
			get {

				if (checklistDetailRepository == null) {
					checklistDetailRepository = new Repository<ChecklistDetail>(databaseContext);
				}
				return checklistDetailRepository;
			}
		}

		public IRepository<TEntity> Repository<TEntity>() where TEntity : Entity {
			string typeName = typeof(TEntity).Name;
			repos.TryGetValue(typeName, out object repo);
			if (repo == null) {
				repo = repositoryFactory.Create<TEntity>();
				repos.Add(typeName, repo);
			}
			return repo as IRepository<TEntity>;
		}

		public void Commit() {
			try {
				databaseContext.SaveChanges();
				transaction.Commit();
			}
			finally {
				transaction.Dispose();
			}
		}

		public void Rollback() {
			transaction.Rollback();
			transaction.Dispose();
		}

		public void StartTransaction() {
			this.transaction = databaseContext.Database.BeginTransaction();
		}


		private bool disposed = false;

		protected virtual void Dispose(bool disposing) {
			if (!this.disposed) {
				if (disposing) {
					databaseContext.Dispose();
				}
			}
			this.disposed = true;
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
