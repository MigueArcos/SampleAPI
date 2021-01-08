using ArchitectureTest.Domain.Repositories.BasicRepo;
using Moq;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchitectureTest.Tests.Shared.Mocks {
    public class MockRepository<TEntity> : Mock<IRepository<TEntity>> where TEntity : class {
        public void SetupGetByIdResult(TEntity result) {
            Setup(repo => repo.GetById(It.IsAny<long>()))
                .Returns(Task.FromResult(result));
        }
        /// <summary>
        /// This method is called when start a search by a field that should return only a single result (Ex. email), like FirstOrDefault
        /// </summary>
        /// <param name="result"></param>
        public void SetupGetSingleResultInList(TEntity result) {
            Setup(repo => repo.Get(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns(Task.FromResult(new List<TEntity> { result }));
        }
        public void SetupGetMultipleResults(List<TEntity> results) {
            Setup(repo => repo.Get(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns(Task.FromResult(results));
        }
        public void SetupPostResult(TEntity result) {
            Setup(repo => repo.Post(It.IsAny<TEntity>()))
                .Returns(Task.FromResult(result));
        }
        public void SetupPutResult(bool result) {
            Setup(repo => repo.Put(It.IsAny<TEntity>()))
                .Returns(Task.FromResult(result));
        }
        public void SetupDeleteResult(bool result) {
            Setup(repo => repo.DeleteById(It.IsAny<long>()))
                .Returns(Task.FromResult(result));
        }
        public void VerifyGetByIdCalls(Times times) {
            Verify(repo => repo.GetById(It.IsAny<long>()), times);
        }
        public void VerifyGetCalls(Times times) {
            Verify(repo => repo.Get(It.IsAny<Expression<Func<TEntity, bool>>>()), times);
        }
        public void VerifyPostCalls(Times times) {
            Verify(repo => repo.Post(It.IsAny<TEntity>()), times);
        }
        public void VerifyPutCalls(Times times) {
            Verify(repo => repo.Put(It.IsAny<TEntity>()), times);
        }
        public void VerifyDeleteCalls(Times times) {
            Verify(repo => repo.DeleteById(It.IsAny<long>()), times);
        }
    }
}
