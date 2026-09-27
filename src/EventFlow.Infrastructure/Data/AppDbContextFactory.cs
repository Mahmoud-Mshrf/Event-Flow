using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EventFlow.Infrastructure.Data;

// EventFlow.Infrastructure/Persistence/AppDbContextFactory.cs
public sealed class AppDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EventFlow;Integrated Security=True;");

        // Null tenant for design-time — filters use null so migrations
        // see all rows (no filter applied at migration generation time)
        return new AppDbContext(
            optionsBuilder.Options,
            new NullCurrentTenant(),
            new NullPublisher());
    }
}
