using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VaiTroConfigurations : IEntityTypeConfiguration<VaiTro>
    {
        public void Configure(EntityTypeBuilder<VaiTro> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenVaiTro).IsRequired().HasMaxLength(50);
            builder.Property(x => x.MoTa).HasMaxLength(200);
        }
    }
}
