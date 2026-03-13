using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NhomQuyenConfigurations : IEntityTypeConfiguration<NhomQuyen>
    {
        public void Configure(EntityTypeBuilder<NhomQuyen> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenNhom).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MoTa).HasMaxLength(200);

            // One-to-Many: NhomQuyen -> Quyen
            builder.HasMany(x => x.Quyens)
                .WithOne(x => x.NhomQuyen)
                .HasForeignKey(x => x.NhomQuyenId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
