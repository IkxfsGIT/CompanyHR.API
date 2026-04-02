mkdir -p Backend/CompanyHR.API/Data
cat > Backend/CompanyHR.API/Data/ApplicationDbContext.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using CompanyHR.API.Models;

namespace CompanyHR.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    // Добавьте другие DbSet по мере необходимости
    // public DbSet<Position> Positions { get; set; }
    // public DbSet<Department> Departments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.HireDate);
        });
    }
}

public DbSet<RefreshToken> RefreshTokens { get; set; }
public DbSet<Position> Positions { get; set; }

EOF
