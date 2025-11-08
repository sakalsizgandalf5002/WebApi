using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;
using Xunit;

namespace Api.IntegrationTests.Infra.Db;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    public MsSqlTestcontainer Container { get; private set; } = default;
    
    public string ConnectionString => Container.ConnectionString;

    private const string Image = "mcr.microsoft.com/mssql/server:2022-latest";
    
    private const string Password = "Str0ng_P@ssw0rd!";

    public async Task InitializeAsync()
    {
        Container = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SA_PASSWORD", "Str0ng_P@ssw0rd!") 
            .WithCleanUp(true)
            .Build();

        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container != null)
            await Container.DisposeAsync();
    }

}