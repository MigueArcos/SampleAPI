
using ArchitectureTest.Data.Database.MySQL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArchitectureTest.Domain.Contracts {
	public interface IEntityConverter<TEntity> where TEntity: Entity {
		TEntity ToEntity();
	}
	public interface IChildEntityConverter<TEntity> where TEntity : Entity {
		List<TEntity> GetChildEntities();
	}
}
