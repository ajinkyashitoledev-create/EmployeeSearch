using EmployeeSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSearch.Infrastructure.Persistence.Configurations;

/// <summary>
/// "Managers" table: PK is also an FK back to Employees.Id (TPT join column).
/// </summary>
public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("Managers", tb =>
            tb.HasCheckConstraint("CK_Managers_TeamSize_NonNegative", "[TeamSize] >= 0"));

        builder.Property(m => m.TeamSize)
            .IsRequired();

        builder.Property(m => m.Bonus)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
