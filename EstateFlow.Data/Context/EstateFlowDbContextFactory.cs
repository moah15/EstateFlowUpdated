using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EstateFlow.Data.Context;

namespace EstateFlow.Data;

public class EstateFlowDbContextFactory : IDesignTimeDbContextFactory<EstateFlowDbContext>
{
    public EstateFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EstateFlowDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=tcp:estateflow.database.windows.net,1433;Initial Catalog=EstateFlowDb;Persist Security Info=False;User ID=estateflow;Password=Insy7315;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

        return new EstateFlowDbContext(optionsBuilder.Options);
    }
}