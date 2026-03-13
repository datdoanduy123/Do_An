using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class QuyTacGiaoViecAIConfigurations : IEntityTypeConfiguration<QuyTacGiaoViecAI>
    {
        public void Configure(EntityTypeBuilder<QuyTacGiaoViecAI> builder)
        {
            builder.ToTable("QuyTacGiaoViecAIs");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.MaQuyTac).IsUnique();

            builder.Property(x => x.MaQuyTac).IsRequired().HasMaxLength(100);
            builder.Property(x => x.GiaTri).IsRequired().HasMaxLength(500);
            builder.Property(x => x.LoaiDuLieu).HasMaxLength(50);
        }
    }
}
