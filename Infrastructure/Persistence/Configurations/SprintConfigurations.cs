using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SprintConfigurations : IEntityTypeConfiguration<Sprint>
    {
        public void Configure(EntityTypeBuilder<Sprint> builder)
        {
            builder.ToTable("Sprints");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenSprint).IsRequired().HasMaxLength(100);
            builder.Property(x => x.TrangThai).HasMaxLength(50);

            // Một Sprint có nhiều Công việc
            builder.HasMany(x => x.CongViecs)
                   .WithOne(x => x.Sprint)
                   .HasForeignKey(x => x.SprintId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
