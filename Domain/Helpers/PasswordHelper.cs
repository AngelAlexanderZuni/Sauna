using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace ProyectoSaunaKalixto.Web.Domain.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Usar el mismo método que WPF para compatibilidad
            return HashPasswordWPF(password);
        }
        
        public static string HashPasswordWPF(string password)
        {
            // Método exacto de WPF: UTF-8 con salt "SaunaSalt2024"
            using (SHA256 sha256 = SHA256.Create())
            {
                string saltedPassword = password + "SaunaSalt2024";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyPassword(string password, string hash)
        {
            // Primero intentar con BCrypt
            if (IsBCryptHash(hash))
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            
            // Si no es BCrypt, intentar con SHA-256 Base64 (formato WPF)
            if (IsBase64Hash(hash))
            {
                return VerifySHA256Base64Password(password, hash);
            }
            
            // Si no es Base64, intentar con SHA-256 HEX (formato antiguo)
            return VerifySHA256HexPassword(password, hash);
        }

        private static bool IsBCryptHash(string hash)
        {
            // Los hashes BCrypt típicamente comienzan con $2a$, $2b$, o $2y$
            return hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$");
        }

        private static bool IsBase64Hash(string hash)
        {
            // Verificar si el hash tiene formato Base64 (longitud múltiplo de 4 y caracteres válidos)
            if (string.IsNullOrEmpty(hash) || hash.Length % 4 != 0)
            {
                return false;
            }
            
            try
            {
                // Intentar convertir de Base64 a bytes
                Convert.FromBase64String(hash);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifySHA256Base64Password(string password, string hash)
        {
            try
            {
                // Método 1: UTF-8 con salt "SaunaSalt2024" - formato WPF exacto
                using (SHA256 sha256 = SHA256.Create())
                {
                    string saltedPassword = password + "SaunaSalt2024";
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                    string computedHash = Convert.ToBase64String(bytes);
                    
                    if (computedHash.Equals(hash, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                
                // Método 2: UTF-16 (Unicode) - formato WPF alternativo
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.Unicode.GetBytes(password));
                    string computedHash = Convert.ToBase64String(bytes);
                    
                    if (computedHash.Equals(hash, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                
                // Método 3: UTF-8 sin salt - formato estándar
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    string computedHash = Convert.ToBase64String(bytes);
                    
                    return computedHash.Equals(hash, StringComparison.Ordinal);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool VerifySHA256HexPassword(string password, string hash)
        {
            try
            {
                // Convertir la contraseña a SHA-256
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    string computedHash = builder.ToString();
                    
                    return computedHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        public static string HashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}