using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence
{
    // Lớp DbContext chính của ứng dụng
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet đại diện cho bảng Users trong cơ sở dữ liệu
        public DbSet<User> Users { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<Quyen> Quyens { get; set; }
        public DbSet<NhomQuyen> NhomQuyens { get; set; }
        public DbSet<NguoiDungVaiTro> NguoiDungVaiTros { get; set; }
        public DbSet<VaiTroQuyen> VaiTroQuyens { get; set; }

        public DbSet<DuAn> DuAns { get; set; }
        public DbSet<TaiLieuDuAn> TaiLieuDuAns { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<CongViec> CongViecs { get; set; }
        public DbSet<DuAnNguoiDung> DuAnNguoiDungs { get; set; }
        public DbSet<NhatKyCongViec> NhatKyCongViecs { get; set; }
        public DbSet<NhomKyNang> NhomKyNangs { get; set; }
        public DbSet<CongNghe> CongNghes { get; set; }
        public DbSet<KyNang> KyNangs { get; set; }
        public DbSet<KyNangNguoiDung> KyNangNguoiDungs { get; set; }
        public DbSet<YeuCauCongViec> YeuCauCongViecs { get; set; }
        public DbSet<QuyTacGiaoViecAI> QuyTacGiaoViecAIs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<TraoLoiCongViec> TraoLoiCongViecs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tự động áp dụng tất cả các cấu hình từ assembly hiện tại (bao gồm UserConfigurations)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
