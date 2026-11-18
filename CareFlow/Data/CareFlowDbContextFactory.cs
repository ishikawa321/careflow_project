using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareFlow.Data;

public class CareFlowDbContextFactory
    : IDesignTimeDbContextFactory<CareFlowDbContext>
{
    public CareFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CareFlowDbContext>()
            .UseSqlite(DatabaseSettings.ConnectionString)
            .Options;

        return new CareFlowDbContext(options);
    }
}