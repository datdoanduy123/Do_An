using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NhatKyCongViecConfigurations : IEntityTypeConfiguration<NhatKyCongViec>
    {
        public void Configure(EntityTypeBuilder<NhatKyCongViec> builder)
        {
            builder.ToTable("NhatKyCongViecs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SoGioLamViec).IsRequired();
            builder.Property(x => x.GhiChu).HasMaxLength(1000);

            builder.HasOne(x => x.CongViec)
                   .WithMany()
                   .HasForeignKey(x => x.CongViecId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.NguoiCapNhat)
                   .WithMany()
                   .HasForeignKey(x => x.NguoiCapNhatId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
