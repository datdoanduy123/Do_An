using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class QuyenConfigurations : IEntityTypeConfiguration<Quyen>
    {
        public void Configure(EntityTypeBuilder<Quyen> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenQuyen).IsRequired().HasMaxLength(100);
            builder.Property(x => x.MaQuyen).IsRequired().HasMaxLength(50);
            builder.Property(x => x.MoTa).HasMaxLength(200);

            builder.HasIndex(x => x.MaQuyen).IsUnique();
        }
    }
}
