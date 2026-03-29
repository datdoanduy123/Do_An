using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class KyNangConfigurations : IEntityTypeConfiguration<KyNang>
    {
        public void Configure(EntityTypeBuilder<KyNang> builder)
        {
            builder.ToTable("KyNangs");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenKyNang).IsRequired().HasMaxLength(100);

            builder.HasOne(x => x.CongNghe)
                .WithMany(c => c.KyNangs)
                .HasForeignKey(x => x.CongNgheId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
