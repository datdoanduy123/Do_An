using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DuAnConfigurations : IEntityTypeConfiguration<DuAn>
    {
        public void Configure(EntityTypeBuilder<DuAn> builder)
        {
            builder.ToTable("DuAns");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenDuAn).IsRequired().HasMaxLength(255);
            builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(50);

            // Cấu hình quan hệ: Một Dự án có nhiều tài liệu
            builder.HasMany(x => x.TaiLieuDuAns)
                   .WithOne(x => x.DuAn)
                   .HasForeignKey(x => x.DuAnId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ: Một Dự án có nhiều Sprint
            builder.HasMany(x => x.Sprints)
                   .WithOne(x => x.DuAn)
                   .HasForeignKey(x => x.DuAnId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Một Dự án có nhiều Công việc
            builder.HasMany(x => x.CongViecs)
                   .WithOne(x => x.DuAn)
                   .HasForeignKey(x => x.DuAnId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Creator)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedBy)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
