using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Peer_Car.Application.Interfaces;

namespace Peer_Car.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveDocumentAsync(IFormFile file, string folderName)
        {
            if (file == null) return string.Empty;

            // تحديد المسار داخل wwwroot
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // توليد اسم فريد للملف
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // إرجاع المسار النسبي لاستخدامه في الـ Database و الـ View
            return $"/uploads/documents/{folderName}/{uniqueFileName}";
        }
    }
}