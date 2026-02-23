using System.Security.Cryptography;
using System.Text;

namespace TrainCoreDiplom.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Пробуем разные кодировки
                byte[] bytes = Encoding.Default.GetBytes(password); // Вместо UTF8
                                                                    // или
                                                                    // byte[] bytes = Encoding.ASCII.GetBytes(password);

                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}