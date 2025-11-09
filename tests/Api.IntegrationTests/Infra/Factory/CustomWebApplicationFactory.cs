using System.Linq;
using Api.Data;
using Api.Models;
using Infra.Auth;
using Api.IntegrationTests.Infra.Db;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests.Infra.Factory;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqlServerContainerFixture _sql;
    
    public CustomWebApplicationFactory(SqlServerContainerFixture sql)
    {
        _sql = sql;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(o =>
            {
                o.UseSqlServer(_sql.ConnectionString,
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.Scheme, _ => { });
            
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            TestSeed.SeedStocks(db);

        });

        
    }
}