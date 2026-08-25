using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSearch.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps the abstract Employee base type to the "Employees" table (root of the
/// TPT hierarchy). Developer and Manager each add their own table joined 1:1
/// on this table's primary key.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", tb =>
            tb.HasCheckConstraint("CK_Employees_Salary_Positive", "[Salary] > 0"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.HireDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(e => e.FullName); // computed in memory, not persisted

        // Enforced uniqueness at the database level, not just in application code.
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("UX_Employees_Email");

        // Speeds up FK lookups and department-scoped searches.
        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        // Speeds up the common "search by name" query path.
        builder.HasIndex(e => new { e.LastName, e.FirstName })
            .HasDatabaseName("IX_Employees_LastName_FirstName");
    }
}
