using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VaiTroQuyenConfigurations : IEntityTypeConfiguration<VaiTroQuyen>
    {
        public void Configure(EntityTypeBuilder<VaiTroQuyen> builder)
        {
            builder.HasKey(x => new { x.VaiTroId, x.QuyenId });

            builder.HasOne(x => x.VaiTro)
                .WithMany(x => x.VaiTroQuyens)
                .HasForeignKey(x => x.VaiTroId);

            builder.HasOne(x => x.Quyen)
                .WithMany(x => x.VaiTroQuyens)
                .HasForeignKey(x => x.QuyenId);
        }
    }
}
