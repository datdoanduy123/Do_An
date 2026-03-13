using System.Collections.Generic;

namespace Apllication.DTOs.CongViec
{
    public class GoiYGiaoViecDto
    {
        public int UserId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public double DiemPhuHop { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public List<string> KyNangPhuHop { get; set; } = new();
    }
}
