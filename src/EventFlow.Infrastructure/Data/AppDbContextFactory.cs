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

        return new AppDbContext(
            optionsBuilder.Options,
            new NullPublisher());
    }
}
