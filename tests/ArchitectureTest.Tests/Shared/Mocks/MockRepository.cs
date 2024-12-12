using ArchitectureTest.Domain.DataAccessLayer.Repositories.BasicRepo;
using Moq;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace ArchitectureTest.Tests.Shared.Mocks;

public class MockRepository<TEntity> : Mock<IRepository<long, TEntity>> where TEntity : class {
    public void SetupGetByIdResult(TEntity result) {
        Setup(repo => repo.GetById(It.IsAny<long>())).ReturnsAsync(result);
    }
    /// <summary>
    /// This method is called when start a search by a field that should return only
    /// a single result (Ex. email), like FirstOrDefault
    /// </summary>
    /// <param name="result"></param>
    public void SetupFindSingleResult(TEntity result) {
        Setup(repo => repo.FindSingle(It.IsAny<Expression<Func<TEntity, bool>>>()))
            .ReturnsAsync(result);
    }
    public void SetupFindMultipleResults(List<TEntity> results) {
        Setup(repo => repo.Find(It.IsAny<Expression<Func<TEntity, bool>>>()))
            .ReturnsAsync(results);
    }
    public void SetupAddEntityResult(TEntity result) {
        Setup(repo => repo.Add(It.IsAny<TEntity>())).ReturnsAsync(result);
    }
    public void SetupUpdateEntityResult(bool result) {
        Setup(repo => repo.Update(It.IsAny<TEntity>()))
            .ReturnsAsync(result);
    }
    public void SetupDeleteResult(bool result) {
        Setup(repo => repo.DeleteById(It.IsAny<long>()))
            .ReturnsAsync(result);
    }
    public void VerifyGetByIdCalls(Times times) {
        Verify(repo => repo.GetById(It.IsAny<long>()), times);
    }
    public void VerifyFindCalls(Times times) {
        Verify(repo => repo.Find(It.IsAny<Expression<Func<TEntity, bool>>>()), times);
    }
    public void VerifyFindSingleCalls(Times times) {
        Verify(repo => repo.FindSingle(It.IsAny<Expression<Func<TEntity, bool>>>()), times);
    }
    public void VerifyAddEntityCalls(Times times) {
        Verify(repo => repo.Add(It.IsAny<TEntity>()), times);
    }
    public void VerifyUpdateEntityCalls(Times times) {
        Verify(repo => repo.Update(It.IsAny<TEntity>()), times);
    }
    public void VerifyDeleteCalls(Times times) {
        Verify(repo => repo.DeleteById(It.IsAny<long>()), times);
    }
}
