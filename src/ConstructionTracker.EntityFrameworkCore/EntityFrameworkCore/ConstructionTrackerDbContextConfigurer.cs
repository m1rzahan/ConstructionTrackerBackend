using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ConstructionTracker.EntityFrameworkCore;

public static class ConstructionTrackerDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ConstructionTrackerDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ConstructionTrackerDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}
