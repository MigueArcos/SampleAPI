using ArchitectureTest.Data.Database.SQLServer.Entities;
using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using ArchitectureTest.Domain.DataAccessLayer.UnitOfWork.RepoFactory;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;

namespace ArchitectureTest.Domain.DataAccessLayer.UnitOfWork; 
public class UnitOfWork : IUnitOfWork, IDisposable {
	private readonly DatabaseContext databaseContext;
	private IDbContextTransaction transaction;
	private IRepositoryFactory repositoryFactory;
	private Dictionary<string, object> repos = new Dictionary<string, object>();
	public UnitOfWork(DatabaseContext databaseContext) {
		this.databaseContext = databaseContext;
		repositoryFactory = new RepositoryFactory(databaseContext);
	}

	public IRepository<TEntity> Repository<TEntity>() where TEntity : class {
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
