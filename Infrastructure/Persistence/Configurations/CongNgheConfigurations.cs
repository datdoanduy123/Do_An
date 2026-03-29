using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CongNgheConfigurations : IEntityTypeConfiguration<CongNghe>
    {
        public void Configure(EntityTypeBuilder<CongNghe> builder)
        {
            builder.ToTable("CongNghes");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenCongNghe).IsRequired().HasMaxLength(150);
            builder.Property(x => x.MoTa).HasMaxLength(500);

            // Mối quan hệ: Một Công nghệ thuộc một Nhóm (đã khai báo ngược ở Nhóm,
            // nhưng khai báo ở đây để tường minh hơn)
            builder.HasOne(x => x.NhomKyNang)
                .WithMany(n => n.CongNghes)
                .HasForeignKey(x => x.NhomKyNangId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mối quan hệ: Một Công nghệ có nhiều Kỹ năng
            builder.HasMany(x => x.KyNangs)
                .WithOne(k => k.CongNghe)
                .HasForeignKey(k => k.CongNgheId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
