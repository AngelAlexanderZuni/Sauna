using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    /// <summary>
    /// Servicio para manejar la subida y gestión de archivos
    /// </summary>
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Extensiones permitidas
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        
        // Tamaño máximo: 5 MB
        private const long MaxFileSize = 5 * 1024 * 1024;

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Guarda un archivo en el servidor y retorna la ruta relativa
        /// </summary>
        /// <param name="file">Archivo a guardar</param>
        /// <param name="folder">Carpeta base (ej: "egresos")</param>
        /// <returns>Ruta relativa del archivo guardado (ej: "/uploads/egresos/2024/11/archivo.pdf")</returns>
        public async Task<string> SaveFileAsync(IFormFile file, string folder = "egresos")
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("El archivo está vacío o no es válido");
            }

            // Validar extensión
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Extensión no permitida. Solo se permiten: {string.Join(", ", _allowedExtensions)}");
            }

            // Validar tamaño
            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException($"El archivo excede el tamaño máximo permitido de {MaxFileSize / 1024 / 1024} MB");
            }

            // Crear estructura de carpetas: uploads/{folder}/{año}/{mes}/
            var now = DateTime.Now;
            var year = now.Year.ToString();
            var month = now.Month.ToString("D2");
            
            var relativePath = Path.Combine("uploads", folder, year, month);
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            // Crear directorio si no existe
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
                _logger.LogInformation("Directorio creado: {Path}", absolutePath);
            }

            // Generar nombre único: timestamp_nombreoriginal
            var timestamp = now.ToString("yyyyMMddHHmmss");
            var safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_")
                .Replace("-", "_");
            
            // Limitar longitud del nombre
            if (safeFileName.Length > 50)
            {
                safeFileName = safeFileName.Substring(0, 50);
            }
            
            var fileName = $"{timestamp}_{safeFileName}{extension}";
            var filePath = Path.Combine(absolutePath, fileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Archivo guardado: {FilePath}", filePath);

            // Retornar ruta relativa con formato web (/)
            var webPath = "/" + Path.Combine(relativePath, fileName).Replace("\\", "/");
            return webPath;
        }

        /// <summary>
        /// Elimina un archivo del servidor
        /// </summary>
        /// <param name="relativePath">Ruta relativa del archivo (ej: "/uploads/egresos/2024/11/archivo.pdf")</param>
        public async Task<bool> DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            try
            {
                // Remover el "/" inicial si existe
                var cleanPath = relativePath.TrimStart('/').Replace("/", "\\");
                var absolutePath = Path.Combine(_environment.WebRootPath, cleanPath);

                if (File.Exists(absolutePath))
                {
                    await Task.Run(() => File.Delete(absolutePath));
                    _logger.LogInformation("Archivo eliminado: {FilePath}", absolutePath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Archivo no encontrado para eliminar: {FilePath}", absolutePath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo: {Path}", relativePath);
                return false;
            }
        }

        /// <summary>
        /// Verifica si una cadena es una ruta de archivo o solo texto
        /// </summary>
        /// <param name="value">Cadena a verificar</param>
        /// <returns>True si parece ser una ruta de archivo</returns>
        public static bool IsFilePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // Si contiene "/" o "\" y tiene una extensión conocida, es probablemente una ruta
            return (value.Contains("/") || value.Contains("\\")) && 
                   (value.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                    value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    value.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtiene la ruta absoluta de un archivo dado su ruta relativa
        /// </summary>
        public string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var cleanPath = relativePath.TrimStart('/').Replace("/", "\\");
            return Path.Combine(_environment.WebRootPath, cleanPath);
        }
    }
}
