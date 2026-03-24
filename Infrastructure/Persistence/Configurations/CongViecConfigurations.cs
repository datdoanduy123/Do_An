using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CongViecConfigurations : IEntityTypeConfiguration<CongViec>
    {
        public void Configure(EntityTypeBuilder<CongViec> builder)
        {
            builder.ToTable("CongViecs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TieuDe).IsRequired().HasMaxLength(500);
            builder.Property(x => x.LoaiCongViec).HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.DoUuTien).HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.TrangThai).HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.PhuongThucGiaoViec).HasConversion<string>().HasMaxLength(50);

            builder.Property(x => x.ThoiGianUocTinh).IsRequired();
            builder.Property(x => x.ThoiGianThucTe).IsRequired(false);
            builder.Property(x => x.NgayBatDauDuKien).IsRequired(false);
            builder.Property(x => x.NgayKetThucDuKien).IsRequired(false);
            builder.Property(x => x.NgayBatDauThucTe).IsRequired(false);
            builder.Property(x => x.NgayKetThucThucTe).IsRequired(false);

            // Quan hệ với người được giao
            builder.HasOne(x => x.Assignee)
                   .WithMany(x => x.CongViecs)
                   .HasForeignKey(x => x.AssigneeId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Creator)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedBy)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
