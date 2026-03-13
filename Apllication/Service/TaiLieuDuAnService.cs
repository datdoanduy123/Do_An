using Apllication.DTOs.TaiLieuDuAn;
using Apllication.IRepositories;
using Apllication.IService;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Apllication.Service
{
    public class TaiLieuDuAnService : ITaiLieuDuAnService
    {
        private readonly ITaiLieuDuAnRepository _repository;
        private readonly IHostEnvironment _env;

        public TaiLieuDuAnService(ITaiLieuDuAnRepository repository, IHostEnvironment env)
        {
            _repository = repository;
            _env = env;
        }

        public async Task<TaiLieuDuAnDto> UploadAsync(int duAnId, IFormFile file, int userId)
        {
            // Đường dẫn lưu trữ file (Trong thư mục uploads của API)
            string uploadPath = Path.Combine(_env.ContentRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            string filePath = Path.Combine(uploadPath, uniqueFileName);

            // Lưu file vào server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lưu thông tin vào Database
            var taiLieu = new TaiLieuDuAn
            {
                DuAnId = duAnId,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = fileExtension,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            var ketQua = await _repository.AddAsync(taiLieu);

            return MapToDto(ketQua);
        }

        public async Task<bool> ProcessWithAiAsync(int taiLieuId)
        {
            var taiLieu = await _repository.GetByIdAsync(taiLieuId);
            if (taiLieu == null || taiLieu.IsProcessed) return false;

            // --- GIẢ LẬP LOGIC AI Ở ĐÂY ---
            // 1. AI đọc nội dung file từ taiLieu.FilePath
            // 2. AI bóc tách các Task, Priority, Story Points...
            // 3. AI tạo các bản ghi CongViec tương ứng
            // 4. AI gán người và chia Sprint (như mô hình chúng ta đã thiết kế)

            // Hiện tại chúng ta đánh dấu là đã xử lý thành công
            taiLieu.IsProcessed = true;
            return await _repository.UpdateAsync(taiLieu);
        }

        public async Task<IEnumerable<TaiLieuDuAnDto>> GetByProjectIdAsync(int projectId)
        {
            var ds = await _repository.GetByProjectIdAsync(projectId);
            return ds.Select(MapToDto);
        }

        private TaiLieuDuAnDto MapToDto(TaiLieuDuAn t)
        {
            return new TaiLieuDuAnDto
            {
                Id = t.Id,
                DuAnId = t.DuAnId,
                FileName = t.FileName,
                FilePath = t.FilePath,
                FileType = t.FileType,
                UploadedBy = t.UploadedBy,
                UploadAt = t.UploadedAt,
                IsProcessed = t.IsProcessed
            };
        }
    }
}
