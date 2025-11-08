using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Respawn;

namespace Api.IntegrationTests.Infra.Db;

public class DbReset
{
    private static Respawner? _respawner;

    public static async Task ResetAsync(string connectionString)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync().ConfigureAwait(false);
        
        _respawner ??= await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = new [] {"dbo"}
        }).ConfigureAwait(false);
        
        await _respawner.ResetAsync(conn).ConfigureAwait(false);
    }
}