using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSearch.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Location)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.CreatedAtUtc)
            .IsRequired();

        // Two departments cannot share the same name.
        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasDatabaseName("UX_Departments_Name");

        builder.HasMany(d => d.Employees)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict); // block deleting a department that still has employees
    }
}
