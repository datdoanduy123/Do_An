using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NhomKyNangConfigurations : IEntityTypeConfiguration<NhomKyNang>
    {
        public void Configure(EntityTypeBuilder<NhomKyNang> builder)
        {
            builder.ToTable("NhomKyNangs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenNhom).IsRequired().HasMaxLength(150);
            builder.Property(x => x.MoTa).HasMaxLength(500);

            // Mối quan hệ: Một Nhóm có nhiều Công nghệ
            builder.HasMany(x => x.CongNghes)
                .WithOne(c => c.NhomKyNang)
                .HasForeignKey(c => c.NhomKyNangId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
