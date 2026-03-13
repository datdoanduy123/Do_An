using Apllication.IService;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    // Lop trien khai tao JWT Token
    public class DichVuToken : IDichVuToken
    {
        private readonly SymmetricSecurityKey _khoa;
        private readonly IConfiguration _cauHinh;

        public DichVuToken(IConfiguration cauHinh)
        {
            _cauHinh = cauHinh;
            var chuoiKhoa = _cauHinh["Jwt:KhoaBimat"];
            if (string.IsNullOrEmpty(chuoiKhoa))
                throw new Exception("Chua cau hinh Jwt:KhoaBimat trong appsettings.json");
                
            _khoa = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chuoiKhoa));
        }

        public string TaoToken(User nguoiDung, List<string> vaiTros)
        {
            // Dinh nghia cac yeu cau (claims) trong Token
            var danhSachYeuCau = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, nguoiDung.Username)
            };

            // Them tat ca vai tro (MaVaiTro) vao Claims
            foreach (var vaiTro in vaiTros)
            {
                danhSachYeuCau.Add(new Claim(ClaimTypes.Role, vaiTro));
            }

            // Tao thong tin ky (signing credentials)
            var thongTinKy = new SigningCredentials(_khoa, SecurityAlgorithms.HmacSha512Signature);

            // Mo ta Token
            var moTaToken = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(danhSachYeuCau),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = thongTinKy,
                Issuer = _cauHinh["Jwt:NguoiPhatHanh"],
                Audience = _cauHinh["Jwt:NguoiDung"]
            };

            // Tao ma Token
            var trinhXuLyToken = new JwtSecurityTokenHandler();
            var token = trinhXuLyToken.CreateToken(moTaToken);

            return trinhXuLyToken.WriteToken(token);
        }
    }
}
