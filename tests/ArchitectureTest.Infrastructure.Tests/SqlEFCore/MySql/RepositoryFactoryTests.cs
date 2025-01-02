using System;
using ArchitectureTest.Infrastructure.SqlEFCore;
using ArchitectureTest.Infrastructure.SqlEFCore.MySql;
using Microsoft.EntityFrameworkCore;
using Xunit;

using DomainEntities = ArchitectureTest.Domain.Entities;
using DatabaseEntities = ArchitectureTest.Databases.MySql.Entities;
using ArchitectureTest.Databases.MySql;
using FluentAssertions;

namespace ArchitectureTest.Infrastructure.Tests.SqlEFCore.MySql;

public class MySqlRepositoryFactoryTests {
    private readonly MySqlRepositoryFactory _systemUnderTest;

    public MySqlRepositoryFactoryTests(){
        var contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "crud")
            .Options;

        _systemUnderTest = new MySqlRepositoryFactory(
            new DatabaseContext(contextOptions), null!
        );
    }

    [Theory]
    [ClassData(typeof(CreateRepoTestData))]
    public void Create_WithValidEntities_ShouldReturnCorrectRepoType(Type domainType, Type expectedRepoType)
    {
        // Arrange
        var methodInfo = typeof(MySqlRepositoryFactory).GetMethod(nameof(MySqlRepositoryFactory.Create))!;
        var genericMethodCaller = methodInfo.MakeGenericMethod(domainType);

        // Act
        var rawResult = genericMethodCaller.Invoke(_systemUnderTest, null);

        // Assert
        rawResult.Should().BeOfType(expectedRepoType);
    }

    internal class CreateRepoTestData : TheoryData<Type, Type>
    {
        public CreateRepoTestData()
        {
            Add(
                typeof(DomainEntities.Note),
                typeof(SqlRepository<DomainEntities.Note, DatabaseEntities.Note>)
            );
            Add(
                typeof(DomainEntities.User),
                typeof(SqlRepository<DomainEntities.User, DatabaseEntities.User>)
            );
            Add(
                typeof(DomainEntities.UserToken),
                typeof(SqlRepository<DomainEntities.UserToken, DatabaseEntities.UserToken>)
            );
            Add(
                typeof(DomainEntities.ChecklistDetail),
                typeof(SqlRepository<DomainEntities.ChecklistDetail, DatabaseEntities.ChecklistDetail>)
            );
            Add(
                typeof(DomainEntities.Checklist),
                typeof(MySqlChecklistRepository)
            );
        }
    }
}
