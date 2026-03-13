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
            builder.Property(x => x.NgayBatDau).IsRequired(false);
            builder.Property(x => x.NgayKetThuc).IsRequired(false);

            // Quan hệ với người được giao
            builder.HasOne(x => x.Assignee)
                   .WithMany()
                   .HasForeignKey(x => x.AssigneeId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
