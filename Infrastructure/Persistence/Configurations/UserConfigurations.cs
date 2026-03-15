using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    // Cấu hình Fluent API cho thực thể User
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {

            // Khóa chính
            builder.HasKey(x => x.Id);

            // Cấu hình các thuộc tính
            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .HasMaxLength(100);

            builder.Property(x => x.DienThoai)
                .HasMaxLength(20);

            builder.Property(x => x.VaiTro)
                .HasMaxLength(50);

            builder.Property(x => x.KhoiLuongCongViec)
                .HasDefaultValue(0);

            // Chỉ mục cho Username để đảm bảo tính duy nhất và tăng tốc độ tìm kiếm
            builder.HasIndex(x => x.Username).IsUnique();
        }
    }
}
