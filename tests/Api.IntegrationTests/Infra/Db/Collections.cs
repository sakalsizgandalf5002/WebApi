using Xunit;

namespace Api.IntegrationTests.Infra.Db;

[CollectionDefinition("sqlserver")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerContainerFixture> { }