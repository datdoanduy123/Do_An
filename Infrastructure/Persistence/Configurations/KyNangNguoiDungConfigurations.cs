using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class KyNangNguoiDungConfigurations : IEntityTypeConfiguration<KyNangNguoiDung>
    {
        public void Configure(EntityTypeBuilder<KyNangNguoiDung> builder)
        {
            builder.ToTable("KyNangNguoiDungs");

            // Cấu hình khóa chính hỗn hợp (Composite Key)
            builder.HasKey(x => new { x.UserId, x.KyNangId });

            builder.HasOne(x => x.User)
                   .WithMany(x => x.KyNangNguoiDungs)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.KyNang)
                   .WithMany(x => x.KyNangNguoiDungs)
                   .HasForeignKey(x => x.KyNangId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
