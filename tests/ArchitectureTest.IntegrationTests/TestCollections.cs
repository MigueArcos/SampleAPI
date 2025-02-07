using Xunit;

namespace ArchitectureTest.IntegrationTests;

[CollectionDefinition("AppRunningWithMySql")]
public class AppRunningWithMySql : ICollectionFixture<MySqlApplicationFactory>;

[CollectionDefinition("AppRunningWithSqlServer")]
public class AppRunningWithSqlServer : ICollectionFixture<SqlServerApplicationFactory>;
