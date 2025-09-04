using Microsoft.EntityFrameworkCore;
using UserManagement.API.Models;

namespace Main.Api.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options) {
    public DbSet<User> Users { get; set; }
    public DbSet<PermissionAssignment> PermissionAssignments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Nationality> Nationalities { get; set; }
    public DbSet<IncomeType> IncomeTypes { get; set; }
    public DbSet<ExpenseType> ExpenseTypes { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Income> Incomes { get; set; }
    public DbSet<UniversityType> UniversityTypes { get; set; }
    public DbSet<ProgramType> ProgramTypes { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<UniversityProgram> UniversityPrograms { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<UniversityApplication> UniversityApplications { get; set; }
    public DbSet<VisaApplication> VisaApplications { get; set; }
    public DbSet<VisaApplicationType> VisaApplicationTypes { get; set; }
    public DbSet<ClientSource> ClientSources { get; set; }
    public DbSet<Community> Communities { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.HasDefaultSchema("V2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
