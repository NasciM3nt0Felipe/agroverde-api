using Microsoft.EntityFrameworkCore;

namespace AgroVerde.Infrastructure.Data;

public class AgroVerdeDbContext : DbContext
{
    public AgroVerdeDbContext(DbContextOptions<AgroVerdeDbContext> options)
        : base(options)
    {
    }
}