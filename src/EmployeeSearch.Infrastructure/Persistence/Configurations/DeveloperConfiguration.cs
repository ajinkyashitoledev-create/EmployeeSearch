using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSearch.Infrastructure.Persistence.Configurations;

/// <summary>
/// "Developers" table: PK is also an FK back to Employees.Id (TPT join column).
/// </summary>
public class DeveloperConfiguration : IEntityTypeConfiguration<Developer>
{
    public void Configure(EntityTypeBuilder<Developer> builder)
    {
        builder.ToTable("Developers");

        builder.Property(d => d.ProgrammingLanguage)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.YearsOfExperience)
            .IsRequired();

        builder.Property(d => d.GitHubProfile)
            .HasMaxLength(200);

        builder.HasIndex(d => d.ProgrammingLanguage)
            .HasDatabaseName("IX_Developers_ProgrammingLanguage");
    }
}
