using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PhuThuocCongViecConfigurations : IEntityTypeConfiguration<PhuThuocCongViec>
    {
        public void Configure(EntityTypeBuilder<PhuThuocCongViec> builder)
        {
            builder.ToTable("PhuThuocCongViecs");

            builder.HasKey(x => x.Id);

            // Cấu hình quan hệ: Task B phụ thuộc vào Task A
            // Task: Task B (Người con)
            builder.HasOne(x => x.Task)
                   .WithMany(x => x.Dependencies)
                   .HasForeignKey(x => x.TaskId)
                   .OnDelete(DeleteBehavior.Cascade);

            // DependsOnTask: Task A (Người tiền đề)
            builder.HasOne(x => x.DependsOnTask)
                   .WithMany()
                   .HasForeignKey(x => x.DependsOnTaskId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
