using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Postgres;

public class FileServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public FileServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("FileService");
        modelBuilder.Ignore<StorageKey>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileServiceDbContext).Assembly);
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(b => b.AddConsole());
    }
}