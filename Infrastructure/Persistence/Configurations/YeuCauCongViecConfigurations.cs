using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class YeuCauCongViecConfigurations : IEntityTypeConfiguration<YeuCauCongViec>
    {
        public void Configure(EntityTypeBuilder<YeuCauCongViec> builder)
        {
            builder.ToTable("YeuCauCongViecs");

            // Cấu hình khóa chính hỗn hợp (Composite Key)
            builder.HasKey(x => new { x.CongViecId, x.KyNangId });

            builder.HasOne(x => x.CongViec)
                   .WithMany(x => x.YeuCauCongViecs)
                   .HasForeignKey(x => x.CongViecId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.KyNang)
                   .WithMany(x => x.YeuCauCongViecs)
                   .HasForeignKey(x => x.KyNangId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
