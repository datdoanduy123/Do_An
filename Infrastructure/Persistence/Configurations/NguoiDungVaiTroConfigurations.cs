using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NguoiDungVaiTroConfigurations : IEntityTypeConfiguration<NguoiDungVaiTro>
    {
        public void Configure(EntityTypeBuilder<NguoiDungVaiTro> builder)
        {
            builder.HasKey(x => new { x.NguoiDungId, x.VaiTroId });

            builder.HasOne(x => x.NguoiDung)
                .WithMany(x => x.NguoiDungVaiTros)
                .HasForeignKey(x => x.NguoiDungId);

            builder.HasOne(x => x.VaiTro)
                .WithMany(x => x.NguoiDungVaiTros)
                .HasForeignKey(x => x.VaiTroId);
        }
    }
}
