using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TaiLieuDuAnConfigurations : IEntityTypeConfiguration<TaiLieuDuAn>
    {
        public void Configure(EntityTypeBuilder<TaiLieuDuAn> builder)
        {
            builder.ToTable("TaiLieuDuAns");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName).IsRequired().HasMaxLength(500);
            builder.Property(x => x.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(x => x.FileType).HasMaxLength(50);
        }
    }
}
