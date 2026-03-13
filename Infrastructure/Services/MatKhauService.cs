using Apllication.IService;
using System.Security.Cryptography;

namespace Infrastructure.Services
{
    // Trien khai ma hoa mat khau bang PBKDF2 voi Salt
    public class MatKhauService : IMatKhauService
    {
        private const int SaltSize = 16; // 128 bit
        private const int HashSize = 32; // 256 bit
        private const int Iterations = 100000; // So lan lap de tang tinh bao mat

        public string TaoPasswordHash(string matKhau)
        {
            // Tao salt ngau nhien
            using var algorithm = new Rfc2898DeriveBytes(
                matKhau,
                SaltSize,
                Iterations,
                HashAlgorithmName.SHA256);

            var salt = Convert.ToBase64String(algorithm.Salt);
            var hash = Convert.ToBase64String(algorithm.GetBytes(HashSize));

            // Luu tru theo dinh dang: Iterations.Salt.Hash
            return $"{Iterations}.{salt}.{hash}";
        }

        public bool XacMinhPassword(string matKhau, string hashDaLuu)
        {
            try
            {
                var parts = hashDaLuu.Split('.', 3);
                if (parts.Length != 3) return false;

                var iterations = int.Parse(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var key = Convert.FromBase64String(parts[2]);

                using var algorithm = new Rfc2898DeriveBytes(
                    matKhau,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256);

                var keyToCheck = algorithm.GetBytes(HashSize);

                // So sanh 2 chuoi byte (dung CryptographicOperations de tranh Timing Attack)
                return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
            }
            catch
            {
                return false;
            }
        }
    }
}
