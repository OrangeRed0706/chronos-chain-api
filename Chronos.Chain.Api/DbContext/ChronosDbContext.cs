using Chronos.Chain.Api.DbContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Chain.Api.DbContext;

public class ChronosDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<TaskInfo> TasksInfo { get; set; }

    public ChronosDbContext()
    {
    }

    public ChronosDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "chronos.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskInfo>().HasKey(x => x.Id);
        base.OnModelCreating(modelBuilder);
    }
}
