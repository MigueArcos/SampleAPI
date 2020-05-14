using System;
using System.Collections.Generic;
using System.Text;
using ArchitectureTest.Data.Database.Entities;
using ArchitectureTest.Data.Repositories.BasicRepo;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArchitectureTest.Data.UnitOfWork {
	public class UnitOfWork : IUnitOfWork, IDisposable {
		private IRepository<Note> notesRepository;
		private IRepository<Checklist> checklistRepository;
		private IRepository<ChecklistDetail> checklistDetailRepository;
		private readonly DatabaseContext databaseContext;
		private IDbContextTransaction transaction;
		public UnitOfWork(DatabaseContext databaseContext) {
			this.databaseContext = databaseContext;
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
					checklistRepository = new Repository<Checklist>(databaseContext);
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
