using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TraoLoiCongViecConfigurations : IEntityTypeConfiguration<TraoLoiCongViec>
    {
        public void Configure(EntityTypeBuilder<TraoLoiCongViec> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.NoiDung)
                .IsRequired()
                .HasMaxLength(2000);

            // Quan hệ với Công việc (Task)
            builder.HasOne(x => x.CongViec)
                .WithMany() // Một Task có nhiều trao đổi
                .HasForeignKey(x => x.CongViecId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ với Người tạo (User)
            builder.HasOne(x => x.NguoiTao)
                .WithMany() // Một User có thể viết nhiều trao đổi
                .HasForeignKey(x => x.NguoiTaoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
